namespace ViviStreamWebsite.Helpers
{
    public static class Constants
    {
        public const string AzureStorageConnectionString = "ConnectionStrings:vivilivestreamstorage_AzureStorageConnectionString";
        public static readonly string QueueTableName = "AzureTableSong";
        public static readonly string AllSongsTableName = "AllSongs";
        public static readonly string FriendsTableName = "Friends";
        public static readonly string BotTimersTableName = "BotTimers";
        public static readonly string CustomCommandsTableName = "CustomCommands";
        public static readonly string StreamInfoTableName = "StreamInfo";
        public static readonly string MySongsTableName = "SongSteals";
        public static readonly string StreamUsersTableName = "StreamUsers";
        public static readonly string CurrentSongTableName = "CurrentSong";
        public static readonly string TagsTableName = "Tags";

        public static readonly string BlobContainerBaseUrl = "https://vivilivestreamstorage.blob.core.windows.net/games/";
        public static readonly string DefaultBlobImage = "default";
        public static readonly int ImageSize = 350;

        public static readonly string ClientSecretsFilename = @"Files\client_id.json";
        public static readonly string ApplicationName = "ViviWebsite";
        public static readonly string ChannelCookieName = "channel_id";
        public static readonly string Rol = "Rol";

        public static string StreamOwner
        {
            get;
            set;
        }

        public static readonly string StreamCommandsFilename = @"Files\stream_commands.json";
        public static readonly string StreamPartitionKey = "a72230c1-96ec-4e17-857b-fbc2d52e8952";
    }
}