namespace Attendify.Common.FacialRecognition;

public sealed class NoMatchingUserException(string message) : Exception(message);