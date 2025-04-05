using System.Reflection;

namespace TwitchManager.Models.Api.Events
{
    public static class TwitchEvents
    {
        public const string ChannelFollow = "channel.follow";
        public const string ChannelSubscribe = "channel.subscribe";
        public const string ChannelCheer = "channel.cheer";
        public const string ChannelRaid = "channel.raid";
        public const string ChannelBan = "channel.ban";
        public const string ChannelUnban = "channel.unban";
        public const string ChannelModeratorAdd = "channel.moderator.add";
        public const string ChannelModeratorRemove = "channel.moderator.remove";
        public const string ChannelPointsCustomRewardAdd = "channel.channel_points_custom_reward.add";
        public const string ChannelPointsCustomRewardUpdate = "channel.channel_points_custom_reward.update";
        public const string ChannelPointsCustomRewardRemove = "channel.channel_points_custom_reward.remove";
        public const string ChannelPointsCustomRewardRedemptionAdd = "channel.channel_points_custom_reward_redemption.add";
        public const string ChannelPointsCustomRewardRedemptionUpdate = "channel.channel_points_custom_reward_redemption.update";
        public const string ChannelPointsCustomRewardRedemptionRemove = "channel.channel_points_custom_reward_redemption.remove";
        public const string StreamOnline = "stream.online";
        public const string StreamOffline = "stream.offline";
        public const string UserAuthorizationGrant = "user.authorization.grant";
        public const string UserAuthorizationRevoke = "user.authorization.revoke";
        public const string UserUpdate = "user.update";
        public const string ExtensionBitsTransactionCreate = "extension.bits_transaction.create";
        public const string ExtensionBitsTransactionUpdate = "extension.bits_transaction.update";
        public const string ExtensionChannelSubscriptions = "extension.channel_subscriptions";
        public const string HypeTrainBegin = "hype_train.begin";
        public const string HypeTrainProgress = "hype_train.progress";
        public const string HypeTrainEnd = "hype_train.end";
        public const string ChannelPredictionBegin = "channel.prediction.begin";
        public const string ChannelPredictionProgress = "channel.prediction.progress";
        public const string ChannelPredictionLock = "channel.prediction.lock";
        public const string ChannelPredictionEnd = "channel.prediction.end";
        public const string ChannelPredictionCancel = "channel.prediction.cancel";
        public const string ChannelPollBegin = "channel.poll.begin";
        public const string ChannelPollProgress = "channel.poll.progress";
        public const string ChannelPollEnd = "channel.poll.end";
        public const string ChannelChatMessage = "channel.chat.message";

        public static IEnumerable<string> GetEvents()
        {
            var events = typeof(TwitchEvents).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .Select(f => f.GetValue(null).ToString());
            
            return events;
        }
    }
}
