# Interactions 
Currently, all connectors transform received data to interactions between employees - a temporal graph model that attempts to unify different types events. In general an interaction is directed edge - a triplet When (Timestamp), From (SourceVertex), To (TargetVertex) with optional additional Properties describing the event or particular interaction. This approach turns out to have some limitations in practice hence the need to describe it in details. Different type of events have different logic to generate interactions.

There’s a special type of Vertex coded (masked) EXTERNAL that represent a person from outside the organization. 

<details>
  <summary>INTERNAL masking (To be considered)</summary>
 A person from inside of organization but outside of deployment scope (e.g. outside the pilot programme or from the capital group) shall be masked INTERNAL. 
</details>

## Meetings

Each small meeting generates interactions between all participants. For example a meeting between A,B,C will generate the following interactions. The logic is indifferent whether A,B or C represent company employees or a person from outside of the organization (EXTERNAL)    
```
A -> B
A -> C
B -> A
B -> C
C -> A
C -> B
```
The number of interactions generated is n*(n-1), and it becomes quite large for big meetings like conferences or company-wide events that can involve thousands of participants and hence generate tens of millions of interactions. Therefore for big meetings the logic for generating interactions is different.

Each big meetings generates only one way interactions from a participant to EXTERNAL. This is consistent with intuition that during an thousand person event not everyone interacts with each other. For instance an event with k participants will generate the following interactions:

```
V1 -> EXTERNAL
V2 -> EXTERNAL
V2 -> EXTERNAL
...
Vk -> EXTERNAL
```

To enable streaming meeting interactions from connector while avoiding generating duplicate interactions, the actual algorithm generating interactions for meeting should iterate through each employee, fetch his calendar and generate only outgoing interactions, unless the other vertex is EXTERNAL (as EXTERNAL will not be included in the employee list). 

Pseudocode:  

interactions = a service that collects interactions and sends them up in batches

```
foreach employee in company.employees (filtered with whitelist and blacklist)
  fetch employee calendar from api
  foreach meeting in employee.calendar.meetings
    if meeting is big then
       interactions.add({ from: employee.id, to: EXTERNAL })
    else // small meeting
       foreach participant in meeting.participant
          if participant != employee 
             interactions.add({ from: employee.id, to: participant.id })
          if perticipant == EXTERNAL
             interactions.add({ from: EXTERNAL, to: employee.id })             
```

Big meeting = meeting with more than 100 participants

## Emails

Each email generate interactions from the sender to all people that received the email. For example an email from A to B, C, D generates the following interactions:

```
A -> B
A -> C
A -> D
```

There is no distinction if the recipient is in TO or CC field of an email envelope.

To enable streaming email interactions from connector while avoiding generating duplicate interactions, the actual algorithm generating interactions for meeting should iterate through each employee, fetch his mailbox and generate only outgoing interactions, unless the other vertex is EXTERNAL (as EXTERNAL will not be included in the employee list). 

Pseudocode:  

interactions = a service that collects interactions and sends them up in batches
```
foreach employee in company.employees (filtered with whitelist and blacklist)
  fetch employee mailbox from api
  foreach email in employee.mailbox
    if email.sender == EXTERNAL
      interactions.add({ from: EXTERNAL, to: employee.id })
    else if email.sender == employee
      foreach recepient in email.recepients
          interactions.add({ from: employee.id, to: recepient.id })
```


## Chats (Slack / MS Teams)

The logic for generating interactions from chats tries to mimic chat platform notifications. If a user action results in a notification sent to other employees it should emit interactions. The following is a description of a slack platform, as the notification logic might be different for other platforms. 

Each new thread message generates interactions from sender to all channel members. 

Each reply generates interactions from sender to all conversation (thread) participants

Each reaction generates interactions from reaction sender to message sender  

Example:    
```
Channel with members A,B,C,D,E
Thread (sender: A)
|- Reply (sender: B)
|- Reply (sender: C)
   |- Reply to reply (sender: A)
      - :) Reaction (sender: E)
```
From the above example we generate the following interactions
```
New thread (line 2): A->B, A->C, A->D, A->E
Reply (line 3): B->A
Reply (line 4): C->A
Reply to reply (line 5): A->C
Reaction: E->A
```

Thread might (and usually do) cross different sync batches, i.e. some messages in a thread appear after it’s part has been already processed in a previous sync. This shouldn’t however change the process.  Consider an example:

Example:

Day 1 (sync 1):    
```
Channel with members A,B,C,D,E
Thread (sender: A, Day 1)
|- Reply (sender: B, Day 2)
```
generates interactions:
```
New thread (line 2): A->B, A->C, A->D, A->E
Reply (line 3): B->A
```
On day 2 more events appear (lines 4-6)
```
Channel with members A,B,C,D,E
Thread (sender: A, Day 1)
|- Reply (sender: B, Day 1)
|- Reply (sender: C, Day 2)
   |- Reply to reply (sender: A, Day 2)
      - :) Reaction (sender: E, Day 2)
```
On a second sync only events from day 2 emit interactions:
```
Reply (line 4): C->A
Reply to reply (line 5): A->C
Reaction: E->A
```
Overall the result should be identical to situation when we process whole thread in a single batch. 

Streaming interactions: similarly to email and calendar sync, the interactions generated from chats can and probably should be streamed to core application during as with this approach we shouldn't generate any duplicate interactions. 


# InteractionId

A unique InteractionId should be passed from connectors through core to analytical module to identify duplicates if any. 

The InteractionId should be constant across different runs for the same interaction emitted, but different for different interactions. A proposed way to generate interactionId:
```
InteractionId = hmac ( concat (event.timestamp, event.id, from, to) ) 
```
where all values are as received from api - i.e. from & to are not hashed used identifiers (one of email, gsuiteId, slackId), timestamp is not bucketed

Consider an email from A to B & C where B & C are EXTERNAL. Such email will generate interactions that look duplicate:
```
A -> EXTERNAL
A -> EXTERNAL
```
However if we add InteractionId they can be distinguished.
```
A -> EXTERNAL (InteractionId = hmac ( concat (2022-12-16T12:00:29, 'XYZ', A, B) ))  
A -> EXTERNAL (InteractionId = hmac ( concat (2022-12-16T12:00:29, 'XYZ', A, C) ))
```
Since hash conflicts are unlikely the uniqueness on the db level should be enforced only for a tuple: 
```
<CompanyId, EventId, Timestamp, InteractionId>
```