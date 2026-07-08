namespace WindowsDev.Infrastructure.Logging
{
    public static class EventIds
    {
        // 1000–1099 Auth / Registration
        public const int RegistrationFailed = 1000;
        public const int AuthorizationFailed = 1001;
        public const int LoginAvailabilityCheckFailed = 1002;
        public const int UsernameAvailabilityCheckFailed = 1003;
        public const int InvalidCredentials = 1004;
        public const int AccountLocked = 1005;

        // 1100–1199 Password Recovery
        public const int PasswordRecoveryFailed = 1100;
        public const int InvalidRecoveryCode = 1101;
        public const int PasswordResetFailed = 1102;

        // 1200–1299 Profile
        public const int ProfileUpdateFailed = 1200;
        public const int PasswordChangeFailed = 1201;
        public const int UsernameChangeFailed = 1202;
        public const int ProfileLoadFailed = 1203;

        // 1300–1399 Projects
        public const int ProjectCreationFailed = 1300;
        public const int ProjectDeleteFailed = 1301;
        public const int ProjectLoadFailed = 1302;
        public const int ProjectUpdateFailed = 1303;
        public const int ProjectNotFound = 1304;

        // 1400–1499 Tasks
        public const int TaskCreationFailed = 1400;
        public const int TaskDeleteFailed = 1401;
        public const int TaskLoadFailed = 1402;
        public const int TaskUpdateFailed = 1403;
        public const int TaskNotFound = 1404;

        // 1500–1599 Comments
        public const int CommentCreationFailed = 1500;
        public const int CommentDeleteFailed = 1501;
        public const int CommentLoadFailed = 1502;
        public const int CommentEmpty = 1503;
        public const int CommentNotFound = 1504;

        // 1600–1699 Attachments
        public const int AttachmentUploadFailed = 1600;
        public const int AttachmentDeleteFailed = 1601;
        public const int AttachmentLoadFailed = 1602;
        public const int AttachmentDownloadFailed = 1603;
        public const int AttachmentNotFound = 1604;
        public const int AttachmentStorageCritical = 1605;
    }
}