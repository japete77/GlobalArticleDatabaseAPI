namespace Core.Exceptions
{
    public static class ExceptionCodes
    {
        // Unhandled exception
        public static int UNHANDLED_EXCEPTION = -1000;

        // Internal exceptions
        public static int INTERNAL_INVALID_API_ARGUMENTS = -2000;
        public static int INTERNAL_ERROR_LOADING_SECURITY_CONFIG = -2001;

        // Database exceptions
        public static int DATABASE_CONNECTION_TIMEOUT = -3000;
        public static int DATABASE_SEQUENCE_NOT_UPDATED = -3001;

        // Collections exceptions
        public static int COLLECTION_DUPLICATED_KEY = -4000;
        public static int COLLECTION_INVALID_NAME = -4001;

        // Identity exceptions
        public static int IDENTITY_ERROR_CREATING_USER = -5000;
        public static int IDENTITY_ERROR_UPDATING_USER = -5001;
        public static int IDENTITY_DUPLICATED_USERNAME = -5002;
        public static int IDENTITY_INVALID_USER_PASSWORD = -5003;
        public static int IDENTITY_AUTHORIZATION_TOKEN_NOT_FOUND = -5004;
        public static int IDENTITY_INVALID_AUTHORIZATION_TOKEN = -5005;
        public static int IDENTITY_USER_NOT_EXIST = -5006;
        public static int IDENTITY_NOT_AUTHORIZED = -5007;
        public static int IDENTITY_AUTHORIZATION_TOKEN_DECRYPTION_ERROR = -5008;
        public static int IDENTITY_AUTHORIZATION_TOKEN_ENCRYPTION_KEY_NOT_FOUND = -5009;
        public static int IDENTITY_AUTHORIZATION_TOKEN_EXPIRED = -5010;
        public static int IDENTITY_AUTHORIZATION_INVALID_AUDIENCE = -5011;
        public static int IDENTITY_AUTHORIZATION_INVALID_ISSUER = -5012;
        public static int IDENTITY_AUTHORIZATION_INVALID_LIFETIME = -5013;
        public static int IDENTITY_AUTHORIZATION_INVALID_SIGNATURE = -5014;
        public static int IDENTITY_AUTHORIZATION_INVALID_SIGNING = -5015;
        public static int IDENTITY_AUTHORIZATION_SECURITY_EXCEPTION = -5016;
        public static int IDENTITY_INVALID_EMAIL = -5017;
        public static int IDENTITY_ERROR_RESET_PASSWORD = -5018;
        public static int IDENTITY_DUPLICATED_EMAIL = -5019;
        public static int IDENTITY_ERROR_DELETING_ADMIN_USER = -5020;
        public static int IDENTITY_ERROR_RENEWING_TOKEN = -5021;

        // Article exceptions
        public static int ARTICLE_NOT_FOUND = -6000;

        // Translation exceptions
        public static int TRANSLATION_ALREADY_EXISTS = -7000;
        public static int TRANSLATION_NOT_FOUND = -7001;
        public static int TRANSLATION_ERROR_UPLOADING_TEXT = -7002;

        // Publication exceptions
        public static int PUBLICATION_ALREADY_EXISTS = -8000;

        // Validation
        public static int VALIDATION_ERROR = -7000;
    }
}
