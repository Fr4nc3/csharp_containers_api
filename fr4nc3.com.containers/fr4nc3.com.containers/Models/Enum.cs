namespace fr4nc3.com.containers.Models
{
    /// <summary>
    /// Error Code Enum
    /// </summary>
    public enum ErrorCode
    {
        EntityAlreadyExist = 1,
        ParameterTooLarge = 2,
        ParameterRequired = 3,
        EntityNoFound = 4,
        ParameterTooSmall = 5,
        ParameterCannotBeNull = 6,
        EntityNoUploaded = 7,
        EntityNoDeleted = 8,
        ServerError = 9
    }
}
