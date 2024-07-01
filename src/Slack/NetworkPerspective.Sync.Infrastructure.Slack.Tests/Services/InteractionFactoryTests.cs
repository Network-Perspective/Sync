using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Mappers;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Services
{
    public class InteractionFactoryTests
    {
        [Fact]
        public void ShouldCreateFromThreadMessage()
        {
            // Arrange
            var dateTimeNow = DateTime.UtcNow;
            const string channelMember1 = "ChannelMember1";
            const string channelMember2 = "ChannelMember2";
            const string channelMember3 = "ChannelMember3";
            const string channelMember4 = "ChannelMember4";

            var employees = new List<Employee>()
                .Add(channelMember1)
                .Add(channelMember2)
                .Add(channelMember3)
                .Add(channelMember4);

            var employeesCollection = new EmployeeCollection(employees, null);

            var channelMembers = new HashSet<string>() { channelMember1, channelMember2, channelMember3, channelMember4 };
            var channelId = Guid.NewGuid().ToString();

            var threadMessage = new ConversationHistoryResponse.SingleMessage()
            {
                User = channelMember1,
                Reactions = new[]
                {
                    new ConversationHistoryResponse.SingleMessage.ReactionToMessage { Users = new[] { channelMember2, channelMember3 } },
                    new ConversationHistoryResponse.SingleMessage.ReactionToMessage { Users = new[] { channelMember2, channelMember4 } }
                },
                TimeStamp = TimeStampMapper.DateTimeToSlackTimeStamp(dateTimeNow)
            };

            // Act
            var reactions = new InteractionFactory(x => $"{x}_hashed", employeesCollection).CreateFromThreadMessage(threadMessage, channelId, channelMembers);

            var threadInteractions = reactions.Where(x => x.UserAction.SetEquals(new HashSet<UserActionType> { UserActionType.Thread }));
            var rectionsInteractions = reactions.Where(x => x.UserAction.SetEquals(new HashSet<UserActionType> { UserActionType.Reaction }));

            threadInteractions.Should().HaveCount(4); //self also included, filtered later
            threadInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember1}_hashed" && x.Target.Id.PrimaryId == $"{channelMember2}_hashed").Should().ContainSingle();
            threadInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember1}_hashed" && x.Target.Id.PrimaryId == $"{channelMember3}_hashed").Should().ContainSingle();
            threadInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember1}_hashed" && x.Target.Id.PrimaryId == $"{channelMember4}_hashed").Should().ContainSingle();

            rectionsInteractions.Should().HaveCount(3);
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember2}_hashed" && x.Target.Id.PrimaryId == $"{channelMember1}_hashed").Should().ContainSingle();
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember3}_hashed" && x.Target.Id.PrimaryId == $"{channelMember1}_hashed").Should().ContainSingle();
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember4}_hashed" && x.Target.Id.PrimaryId == $"{channelMember1}_hashed").Should().ContainSingle();
        }

        [Fact]
        public void ShouldCreateFromThreadReplies()
        {
            // Arrange
            var dateTimeNow = DateTime.UtcNow;
            const string channelMember1 = "ChannelMember1";
            const string channelMember2 = "ChannelMember2";
            const string channelMember3 = "ChannelMember3";
            const string channelMember4 = "ChannelMember4";

            var employees = new List<Employee>()
                .Add(channelMember1)
                .Add(channelMember2)
                .Add(channelMember3)
                .Add(channelMember4);

            var employeesCollection = new EmployeeCollection(employees, null);

            var channelMembers = new HashSet<string>() { channelMember1, channelMember2, channelMember3, channelMember4 };
            var channelId = Guid.NewGuid().ToString();
            var threadId = Guid.NewGuid().ToString();

            var threadReply1 = new ConversationRepliesResponse.SingleMessage()
            {
                User = channelMember1,
                Reactions = new[]
                {
                    new ConversationRepliesResponse.SingleMessage.ReactionToMessage { Users = new[] { channelMember2 } },
                },
                TimeStamp = TimeStampMapper.DateTimeToSlackTimeStamp(dateTimeNow)
            };

            var threadReply2 = new ConversationRepliesResponse.SingleMessage()
            {
                User = channelMember2,
                Reactions = new[]
                {
                    new ConversationRepliesResponse.SingleMessage.ReactionToMessage { Users = new[] { channelMember3 } },
                    new ConversationRepliesResponse.SingleMessage.ReactionToMessage { Users = new[] { channelMember4 } }
                },
                TimeStamp = TimeStampMapper.DateTimeToSlackTimeStamp(dateTimeNow)
            };

            // Act
            var reactions = new InteractionFactory(x => $"{x}_hashed", employeesCollection).CreateFromThreadReplies(new[] { threadReply1, threadReply2 }, channelId, threadId, channelMember4, new TimeRange(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1)));

            var threadInteractions = reactions.Where(x => x.UserAction.SetEquals(new HashSet<UserActionType> { UserActionType.Thread }));
            var replyInteractions = reactions.Where(x => x.UserAction.SetEquals(new HashSet<UserActionType> { UserActionType.Reply }));
            var rectionsInteractions = reactions.Where(x => x.UserAction.SetEquals(new HashSet<UserActionType> { UserActionType.Reaction }));

            threadInteractions.Should().BeEmpty();

            replyInteractions.Should().HaveCount(3); //self also included, filtered later
            replyInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember1}_hashed" && x.Target.Id.PrimaryId == $"{channelMember4}_hashed").Should().ContainSingle();
            replyInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember2}_hashed" && x.Target.Id.PrimaryId == $"{channelMember1}_hashed").Should().ContainSingle();
            replyInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember2}_hashed" && x.Target.Id.PrimaryId == $"{channelMember4}_hashed").Should().ContainSingle();

            rectionsInteractions.Should().HaveCount(3);
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember2}_hashed" && x.Target.Id.PrimaryId == $"{channelMember1}_hashed").Should().ContainSingle();
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember3}_hashed" && x.Target.Id.PrimaryId == $"{channelMember2}_hashed").Should().ContainSingle();
            rectionsInteractions.Where(x => x.Source.Id.PrimaryId == $"{channelMember4}_hashed" && x.Target.Id.PrimaryId == $"{channelMember2}_hashed").Should().ContainSingle();
        }
    }
}