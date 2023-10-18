# Custom integration

This document describes basic file format that is accepted by Network Perspective CLI connector for implementing custom - usually scripted - integrations. 
The files described should adhere to CSV specifications https://datatracker.ietf.org/doc/html/rfc4180 this includes CRLF line endings and escaping, but might use other than comma (,) field delimiter (for improved readability of encoded lists).

## List of employees 

This file contains information about employees and teams or groups they belong to. 

It should be exported monthly. 
Example file contents `2023-06_employees.csv` should look similar to the table below  [or a raw csv](./custom-integration/2023-06_employees.csv).

| row_date  | employee_id  | supervisor_id | email       | employment_date | team_id | group_ids                     |
|-----------|--------------|---------------|-------------|-----------------|---------|-------------------------------|
| 2023-06-30| 39954af95558 | fbc3e7b8e8d7  | a440e59f68fa| 2023-03-01      | a9f6b2c1      | ['a9f6b2c1', 'd3e4f5a6', 'b7c8d9e0', 'f0a1b2c3', 'e2d3f4a5'] |
| 2023-06-30| 6db177e434bb | 39954af95558  | fbc3e7b8e8d7| 2023-04-01      | f0a1b2c3      | ['f0a1b2c3', 'd3e4f5a6', 'b7c8d9e0', 'a2b3c4d5']           |

Fields in the CSV table:
* row_date 
  * date when the row info gathered yyyy-MM-dd
* employee_id
  * employee unique identifier
  * it might be an email if other identifier is not available
* supervisor_id
  * supervisor unique identifier
  * it might be an email if other identifier is not available
* email
  * employee email 
  * to join this information with other connectors / sources 
* employment_date
  * date of employment yyyy-MM-dd
  * recommended but optional. Please skip the field (instead of sending empty value) if information is not available.
  * the date might be rounded to 1st day of month to preserve privacy
* team_id
  * id of employee team
  * used to distinguish intra-team vs cross-team collaboration
* group_ids
  * list of ids of other groups employee belongs to
  * json encoded list of strings e.g. ['f0a1b2c3', 'd3e4f5a6', 'b7c8d9e0', 'a2b3c4d5'] or ["f0a1b2c3", "d3e4f5a6", "b7c8d9e0", "a2b3c4d5"]
  * other groups are departments, divisions, agile squads, etc. Groups map to a reports in user interface – if we want to see reports about agile squads, please include the agile squad id in this list. 

All fields except dates (row_date & employment_date) shall be hashed with HMAC algorithm and customer key before sent to Network Perspective. It can be done with a powershell script provided. Each group_id in the field group_ids shall be hashed individually yielding list of hashed identifiers. 


## List of groups

This file contains unhashed names of groups & teams within the company that will map to reports visible in Network Perspective UI. 
Example file contents `2023-06_employees_groups.csv` should look similar to the table below  [or a raw csv](./custom-integration/2023-06_employees_groups.csv).

| id | name                       | category    | parent_id |
|----|----------------------------|-------------|-----------|
| a9f6b2c1  | Devops Team                | Team        | 444e5f6a         |
| d3e4f5a6  | Compliance Department      | Department  | 8deef6a7         |
| b7c8d9e0  | Marketing Division         | Division    | c3e4f5a6         |
| e2d3f4a5  | Engineering Team           | Team        | b7c8d9e0         |
| a2b3c4d5  | Quality Assurance Team     | Team        | b7c8d9e0         |
| c6d7e8f9  | Human Resources Department | Department  | c3e4f5a6         |
| 8d4e5f6a  | Finance Department         | Department  | c3e4f5a6         |

Fields in the CSV table:
* id 
  * identifier of a group or team employee might belong to
  * these are the same identifiers as team_id & group_id
* name 
  * name of the group or team
  * this becomes a title of the report 
* category
  * group category (e.g. Team, Department, Agile Squad, Project, etc.) 
  * reports are organized in categories for better browsing experience. Also if necessary each category might use slightly different report template, use a bit different wording, etc.
* parent_id
  * reference to an id field above
  * used to organize groups into tree like hierarchy 

Fields id & parent_id shall be hashed with HMAC algorithm and customer key before sent to Network Perspective. It can be done with a powershell script provided. 

 
## Users & permissions
We might want to automate synchronization of application users and their permissions. This is optional as users can be also created and assigned permissions via admin UI. However, if there are more than dozens of users to be managed it is a good practice to automate this process in the long run.

Example file contents `2023-06_users.csv` should look similar to the table below  [or a raw csv](./custom-integration/2023-06_users.csv).

| email                  | report_ids                                  |
|------------------------|---------------------------------------------|
| john.doe@example.com   | ['a9f6b2c1']                                |
| anna.smith@example.com | ['d3e4f5a6']                                |
| mary.jones@example.com | ['b7c8d9e0']                     |
| paul.taylor@example.com| ['e2d3f4a5', 'a2b3c4d5', 'c6d7e8f9']         |
| kate.wilson@example.com| ['d4e5f6a7']                                |

Fields in the CSV table:

* email
  * email address (login) of a user that should have access to the application
  * here email address is NOT hashed as it will be used in application login process
* report_ids
  * list of ids of groups (reports) the user shall have access to
  * Each group_id in the field group_ids shall be hashed individually yielding list of hashed identifiers.
