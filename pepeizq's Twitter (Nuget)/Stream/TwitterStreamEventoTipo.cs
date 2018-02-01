using System.Runtime.Serialization;

namespace pepeizq.Twitter.Stream
{
    public enum TwitterStreamEventoTipo
    {
        Unknown,

        [EnumMember(Value = "block")]
        Block,

        [EnumMemberAttribute(Value = "unblock")]
        Unblock,

        [EnumMemberAttribute(Value = "favorite")]
        Favorite,

        [EnumMemberAttribute(Value = "unfavorite")]
        Unfavorite,

        [EnumMemberAttribute(Value = "follow")]
        Follow,

        [EnumMemberAttribute(Value = "unfollow")]
        Unfollow,

        [EnumMemberAttribute(Value = "list_member_added")]
        ListMemberAdded,

        [EnumMemberAttribute(Value = "list_member_removed")]
        ListMemberRemoved,

        [EnumMemberAttribute(Value = "list_user_subscribed")]
        ListUserSubscribed,

        [EnumMemberAttribute(Value = "list_user_unsubscribed")]
        ListUserUnsubscribed,

        [EnumMemberAttribute(Value = "list_created")]
        ListCreated,

        [EnumMemberAttribute(Value = "list_updated")]
        ListUpdated,

        [EnumMemberAttribute(Value = "list_destroyed")]
        ListDestroyed,

        [EnumMemberAttribute(Value = "user_update")]
        UserUpdated,

        [EnumMemberAttribute(Value = "access_revoked")]
        AccessRevoked,

        [EnumMemberAttribute(Value = "quoted_tweet")]
        QuotedTweet,

        [EnumMemberAttribute(Value = "retweeted_retweet")]
        RetweetTweet,

        [EnumMemberAttribute(Value = "favorited_retweet")]
        FavoritedTweet
    }
}
