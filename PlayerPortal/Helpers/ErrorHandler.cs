namespace PlayerPortal.Helpers
{
    public static class ErrorHandler
    {
        public static string GenerateErrorMessage(string operation, int? playerId = null, Exception? innerException = null)
        {
            string baseMessage = playerId.HasValue
                ? $"An error occurred while {operation} the player with ID {playerId}."
                : $"An error occurred while {operation} the player.";

            if (innerException != null)
            {
                baseMessage += $" Details: {innerException.Message}";
            }

            return baseMessage;
        }

        public static string GenerateFailureMessage(string operation, int? playerId = null, string? failureMessage = null)
        {
            string baseMessage = playerId.HasValue
                ? $"Failed to {operation} player with ID {playerId}"
                : $"Failed to {operation} player";

            if (!string.IsNullOrEmpty(failureMessage))
            {
                baseMessage += $": {failureMessage}";
            }
            else
            {
                baseMessage += ".";
            }

            return baseMessage;
        }
    }
}