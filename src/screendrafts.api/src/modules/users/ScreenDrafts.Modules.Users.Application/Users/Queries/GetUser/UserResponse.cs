﻿namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUser;

public sealed record UserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string MiddleName,
    string LastName);
