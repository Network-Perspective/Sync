namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client
{
    internal static class SampleResponse
    {
        public const string GetConversationsList = @"{
                ""ok"": true,
                ""channels"": [
                    {
                        ""id"": ""C012AB3CD"",
                        ""name"": ""general"",
                        ""is_channel"": true,
                        ""is_group"": false,
                        ""is_im"": false,
                        ""created"": 1449252889,
                        ""creator"": ""U012A3CDE"",
                        ""is_archived"": false,
                        ""is_general"": true,
                        ""unlinked"": 0,
                        ""name_normalized"": ""general"",
                        ""is_shared"": false,
                        ""is_ext_shared"": false,
                        ""is_org_shared"": false,
                        ""pending_shared"": [],
                        ""is_pending_ext_shared"": false,
                        ""is_member"": true,
                        ""is_private"": false,
                        ""is_mpim"": false,
                        ""topic"": {
                            ""value"": ""Company-wide announcements and work-based matters"",
                            ""creator"": """",
                            ""last_set"": 0
                        },
                        ""purpose"": {
                            ""value"": ""This channel is for team-wide communication and announcements. All team members are in this channel."",
                            ""creator"": """",
                            ""last_set"": 0
                        },
                        ""previous_names"": [],
                        ""num_members"": 4
                    },
                    {
                        ""id"": ""C061EG9T2"",
                        ""name"": ""random"",
                        ""is_channel"": true,
                        ""is_group"": false,
                        ""is_im"": false,
                        ""created"": 1449252889,
                        ""creator"": ""U061F7AUR"",
                        ""is_archived"": false,
                        ""is_general"": false,
                        ""unlinked"": 0,
                        ""name_normalized"": ""random"",
                        ""is_shared"": false,
                        ""is_ext_shared"": false,
                        ""is_org_shared"": false,
                        ""pending_shared"": [],
                        ""is_pending_ext_shared"": false,
                        ""is_member"": true,
                        ""is_private"": false,
                        ""is_mpim"": false,
                        ""topic"": {
                            ""value"": ""Non-work banter and water cooler conversation"",
                            ""creator"": """",
                            ""last_set"": 0
                        },
                        ""purpose"": {
                            ""value"": ""A place for non-work-related flimflam, faffing, hodge-podge or jibber-jabber you'd prefer to keep out of more focused work-related channels."",
                            ""creator"": """",
                            ""last_set"": 0
                        },
                        ""previous_names"": [],
                        ""num_members"": 4
                    }
                ],
                ""response_metadata"": {
                    ""next_cursor"": ""dGVhbTpDMDYxRkE1UEI=""
                }
            }";

        public const string GetConversationMembers = @"{
            ""ok"": true,
            ""members"": [
                ""U023BECGF"",
                ""U061F7AUR"",
                ""W012A3CDE""
            ],
            ""response_metadata"": {
                ""next_cursor"": ""e3VzZXJfaWQ6IFcxMjM0NTY3fQ==""
            }
        }";

        public const string GetConversationHistory = @"{
            ""ok"": true,
            ""messages"": [
                {
                    ""type"": ""message"",
                    ""user"": ""U012AB3CDE"",
                    ""text"": ""I find you punny and would like to smell your nose letter"",
                    ""ts"": ""1512085950.000216""
                },
                {
                    ""type"": ""message"",
                    ""user"": ""U061F7AUR"",
                    ""text"": ""Isn't this whether dreadful? <https://badpuns.example.com/puns/123>"",
                    ""attachments"": [
                        {
                            ""service_name"": ""Leg end nary a laugh, Ink."",
                            ""text"": ""This is likely a pun about the weather."",
                            ""fallback"": ""We're withholding a pun from you"",
                            ""thumb_url"": ""https://badpuns.example.com/puns/123.png"",
                            ""thumb_width"": 1920,
                            ""thumb_height"": 700,
                            ""id"": 1
                        }
                    ],
                    ""ts"": ""1512085950.218404""
                }
            ],
            ""has_more"": true,
            ""pin_count"": 0,
            ""response_metadata"": {
                ""next_cursor"": ""bmV4dF90czoxNTEyMTU0NDA5MDAwMjU2""
            }
        }";

        public const string GetReplies = @"{
            ""messages"": [
                {
                    ""type"": ""message"",
                    ""user"": ""U061F7AUR"",
                    ""text"": ""island"",
                    ""thread_ts"": ""1482960137.003543"",
                    ""reply_count"": 3,
                    ""subscribed"": true,
                    ""last_read"": ""1484678597.521003"",
                    ""unread_count"": 0,
                    ""ts"": ""1482960137.003543""
                },
                {
                    ""type"": ""message"",
                    ""user"": ""U061F7AUR"",
                    ""text"": ""one island"",
                    ""thread_ts"": ""1482960137.003543"",
                    ""parent_user_id"": ""U061F7AUR"",
                    ""ts"": ""1483037603.017503""
                },
                {
                    ""type"": ""message"",
                    ""user"": ""U061F7AUR"",
                    ""text"": ""two island"",
                    ""thread_ts"": ""1482960137.003543"",
                    ""parent_user_id"": ""U061F7AUR"",
                    ""ts"": ""1483051909.018632""
                },
                {
                    ""type"": ""message"",
                    ""user"": ""U061F7AUR"",
                    ""text"": ""three for the land"",
                    ""thread_ts"": ""1482960137.003543"",
                    ""parent_user_id"": ""U061F7AUR"",
                    ""ts"": ""1483125339.020269""
                }
            ],
            ""has_more"": true,
            ""ok"": true,
            ""response_metadata"": {
                ""next_cursor"": ""bmV4dF90czoxNDg0Njc4MjkwNTE3MDkx""
            }
        }";

        public const string JoinConversation = @"{
            ""ok"": true,
            ""channel"": {
                ""id"": ""C061EG9SL"",
                ""name"": ""general"",
                ""is_channel"": true,
                ""is_group"": false,
                ""is_im"": false,
                ""created"": 1449252889,
                ""creator"": ""U061F7AUR"",
                ""is_archived"": false,
                ""is_general"": true,
                ""unlinked"": 0,
                ""name_normalized"": ""general"",
                ""is_shared"": false,
                ""is_ext_shared"": false,
                ""is_org_shared"": false,
                ""pending_shared"": [],
                ""is_pending_ext_shared"": false,
                ""is_member"": true,
                ""is_private"": false,
                ""is_mpim"": false,
                ""topic"": {
                    ""value"": ""Which widget do you worry about?"",
                    ""creator"": """",
                    ""last_set"": 0
                },
                ""purpose"": {
                    ""value"": ""For widget discussion"",
                    ""creator"": """",
                    ""last_set"": 0
                },
                ""previous_names"": []
            },
            ""warning"": ""already_in_channel"",
            ""response_metadata"": {
                ""warnings"": [
                    ""already_in_channel""
                ]
            }
        }";

        public const string GetUsersList = @"{
            ""ok"": true,
            ""members"": [
                {
                    ""id"": ""W012A3CDE"",
                    ""team_id"": ""T012AB3C4"",
                    ""name"": ""spengler"",
                    ""deleted"": false,
                    ""color"": ""9f69e7"",
                    ""real_name"": ""spengler"",
                    ""tz"": ""America/Los_Angeles"",
                    ""tz_label"": ""Pacific Daylight Time"",
                    ""tz_offset"": -25200,
                    ""profile"": {
                        ""avatar_hash"": ""ge3b51ca72de"",
                        ""status_text"": ""Print is dead"",
                        ""status_emoji"": "":books:"",
                        ""real_name"": ""Egon Spengler"",
                        ""display_name"": ""spengler"",
                        ""real_name_normalized"": ""Egon Spengler"",
                        ""display_name_normalized"": ""spengler"",
                        ""email"": ""spengler@ghostbusters.example.com"",
                        ""image_24"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""image_32"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""image_48"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""image_72"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""image_192"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""image_512"": ""https://.../avatar/e3b51ca72dee4ef87916ae2b9240df50.jpg"",
                        ""team"": ""T012AB3C4""
                    },
                    ""is_admin"": true,
                    ""is_owner"": false,
                    ""is_primary_owner"": false,
                    ""is_restricted"": false,
                    ""is_ultra_restricted"": false,
                    ""is_bot"": false,
                    ""updated"": 1502138686,
                    ""is_app_user"": false,
                    ""has_2fa"": false
                },
                {
                    ""id"": ""W07QCRPA4"",
                    ""team_id"": ""T0G9PQBBK"",
                    ""name"": ""glinda"",
                    ""deleted"": false,
                    ""color"": ""9f69e7"",
                    ""real_name"": ""Glinda Southgood"",
                    ""tz"": ""America/Los_Angeles"",
                    ""tz_label"": ""Pacific Daylight Time"",
                    ""tz_offset"": -25200,
                    ""profile"": {
                        ""avatar_hash"": ""8fbdd10b41c6"",
                        ""image_24"": ""https://a.slack-edge.com...png"",
                        ""image_32"": ""https://a.slack-edge.com...png"",
                        ""image_48"": ""https://a.slack-edge.com...png"",
                        ""image_72"": ""https://a.slack-edge.com...png"",
                        ""image_192"": ""https://a.slack-edge.com...png"",
                        ""image_512"": ""https://a.slack-edge.com...png"",
                        ""image_1024"": ""https://a.slack-edge.com...png"",
                        ""image_original"": ""https://a.slack-edge.com...png"",
                        ""first_name"": ""Glinda"",
                        ""last_name"": ""Southgood"",
                        ""title"": ""Glinda the Good"",
                        ""phone"": """",
                        ""skype"": """",
                        ""real_name"": ""Glinda Southgood"",
                        ""real_name_normalized"": ""Glinda Southgood"",
                        ""display_name"": ""Glinda the Fairly Good"",
                        ""display_name_normalized"": ""Glinda the Fairly Good"",
                        ""email"": ""glenda@south.oz.coven""
                    },
                    ""is_admin"": true,
                    ""is_owner"": false,
                    ""is_primary_owner"": false,
                    ""is_restricted"": false,
                    ""is_ultra_restricted"": false,
                    ""is_bot"": false,
                    ""updated"": 1480527098,
                    ""has_2fa"": false
                }
            ],
            ""cache_ts"": 1498777272,
            ""response_metadata"": {
                ""next_cursor"": ""dXNlcjpVMEc5V0ZYTlo=""
            }
        }";

        public const string GetUsersConversations = @"{
            ""ok"": true,
            ""channels"": [
                {
                    ""id"": ""C012AB3CD"",
                    ""name"": ""general"",
                    ""is_channel"": true,
                    ""is_group"": false,
                    ""is_im"": false,
                    ""created"": 1449252889,
                    ""creator"": ""U012A3CDE"",
                    ""is_archived"": false,
                    ""is_general"": true,
                    ""unlinked"": 0,
                    ""name_normalized"": ""general"",
                    ""is_shared"": false,
                    ""is_ext_shared"": false,
                    ""is_org_shared"": false,
                    ""pending_shared"": [],
                    ""is_pending_ext_shared"": false,
                    ""is_private"": false,
                    ""is_mpim"": false,
                    ""topic"": {
                        ""value"": ""Company-wide announcements and work-based matters"",
                        ""creator"": """",
                        ""last_set"": 0
                    },
                    ""purpose"": {
                        ""value"": ""This channel is for team-wide communication and announcements. All team members are in this channel."",
                        ""creator"": """",
                        ""last_set"": 0
                    },
                    ""previous_names"": []
                },
                {
                    ""id"": ""C061EG9T2"",
                    ""name"": ""random"",
                    ""is_channel"": true,
                    ""is_group"": false,
                    ""is_im"": false,
                    ""created"": 1449252889,
                    ""creator"": ""U061F7AUR"",
                    ""is_archived"": false,
                    ""is_general"": false,
                    ""unlinked"": 0,
                    ""name_normalized"": ""random"",
                    ""is_shared"": false,
                    ""is_ext_shared"": false,
                    ""is_org_shared"": false,
                    ""pending_shared"": [],
                    ""is_pending_ext_shared"": false,
                    ""is_private"": false,
                    ""is_mpim"": false,
                    ""topic"": {
                        ""value"": ""Non-work banter and water cooler conversation"",
                        ""creator"": """",
                        ""last_set"": 0
                    },
                    ""purpose"": {
                        ""value"": ""A place for non-work-related flimflam, faffing, hodge-podge or jibber-jabber you'd prefer to keep out of more focused work-related channels."",
                        ""creator"": """",
                        ""last_set"": 0
                    },
                    ""previous_names"": []
                }
            ],
            ""response_metadata"": {
                ""next_cursor"": ""dGVhbTpDMDYxRkE1UEI=""
            }
        }";

        public const string GetReactions = @"{
            ""file"": {
                ""channels"": [
                    ""C2U7V2YA2""
                ],
                ""comments_count"": 1,
                ""created"": 1507850315,
                ""groups"": [],
                ""id"": ""F7H0D7ZA4"",
                ""ims"": [],
                ""name"": ""computer.gif"",
                ""reactions"": [
                    {
                        ""count"": 1,
                        ""name"": ""stuck_out_tongue_winking_eye"",
                        ""users"": [
                            ""U2U85N1RV""
                        ]
                    }
                ],
                ""timestamp"": 1507850315,
                ""title"": ""computer.gif"",
                ""user"": ""U2U85N1RV""
            },
            ""ok"": true,
            ""type"": ""file""
        }";

        public const string Access = @"{
            ""ok"": true,
            ""access_token"": ""xoxb-17653672481-19874698323-pdFZKVeTuE8sk7oOcBrzbqgy"",
            ""token_type"": ""bot"",
            ""scope"": ""commands,incoming-webhook"",
            ""bot_user_id"": ""U0KRQLJ9H"",
            ""app_id"": ""A0KRD7HC3"",
            ""team"": {
                ""name"": ""Slack Softball Team"",
                ""id"": ""T9TK3CUKW""
            },
            ""enterprise"": {
                ""name"": ""slack-sports"",
                ""id"": ""E12345678""
            },
            ""authed_user"": {
                ""id"": ""U1234"",
                ""scope"": ""chat:write"",
                ""access_token"": ""xoxp-1234"",
                ""token_type"": ""user""
            }
        }";

        public const string ConversationsList = @"{
            ""ok"": true,
            ""teams"": [
                {
                    ""name"": ""Shinichi's workspace"",
                    ""id"": ""T12345678""
                },
                {
                    ""name"": ""Migi's workspace"",
                    ""id"": ""T12345679""
                }
            ],
            ""response_metadata"": {
                ""next_cursor"": ""dXNlcl9pZDo5MTQyOTI5Mzkz""
            }
        }";
    }
}