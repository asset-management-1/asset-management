namespace Authentication.Application.Queries.UserInfo;

/// <summary>
/// Represents a request to load current authenticated user information.
/// </summary>
public class GetUserInfoQuery : IQuery<ResponseDto<UserInfoResponseDto>>;
