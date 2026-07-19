namespace Authentication.Api.Swagger.Examples;

/// <summary>
/// Groups Swagger request and response examples for Authentication API endpoints.
/// </summary>
public static class AuthenticationSwaggerExamples
{
    /// <summary>
    /// Example request for local login.
    /// </summary>
    public sealed class LoginRequest : SwaggerExampleProvider<LoginCommand>
    {
        /// <summary>
        /// Builds a local-login request example.
        /// </summary>
        /// <returns>The local-login request example.</returns>
        protected override LoginCommand BuildExample()
        {
            // Use fake credentials; examples must never contain real passwords.
            return new LoginCommand
            {
                UserName = "tenant@example.com",
                Password = "P@ssw0rd!23"
            };
        }
    }

    /// <summary>
    /// Example request for account registration.
    /// </summary>
    public sealed class RegisterRequest : SwaggerExampleProvider<RegisterCommand>
    {
        /// <summary>
        /// Builds a registration request example.
        /// </summary>
        /// <returns>The registration request example.</returns>
        protected override RegisterCommand BuildExample()
        {
            // PartyType is an enum in the API contract and is normalised by Application mapping after validation.
            return new RegisterCommand
            {
                UserName = "tenant@example.com",
                PartyType = PartyTypeEnum.Tenant,
                Password = "P@ssw0rd!23",
                Email = "tenant@example.com",
                PhoneNumber = AuthenticationSwaggerExampleConstants.EXAMPLE_PHONE_NUMBER,
                FullName = "Nguyen Van A"
            };
        }
    }

    /// <summary>
    /// Example request for register-email OTP verification.
    /// </summary>
    public sealed class VerifyRegisterEmailRequest : SwaggerExampleProvider<VerifyRegisterEmailCommand>
    {
        /// <summary>
        /// Builds a register-email OTP verification request example.
        /// </summary>
        /// <returns>The register-email OTP verification request example.</returns>
        protected override VerifyRegisterEmailCommand BuildExample()
        {
            // OTP is fake and intentionally short because the configured Haven OTP length is 4 digits.
            return new VerifyRegisterEmailCommand
            {
                Email = "tenant@example.com",
                Otp = "1234"
            };
        }
    }

    /// <summary>
    /// Example request for refresh-token rotation.
    /// </summary>
    public sealed class RefreshTokenRequest : SwaggerExampleProvider<RefreshTokenCommand>
    {
        /// <summary>
        /// Builds a refresh-token request example.
        /// </summary>
        /// <returns>The refresh-token request example.</returns>
        protected override RefreshTokenCommand BuildExample()
        {
            // Refresh token is a masked fake value for documentation only.
            return new RefreshTokenCommand
            {
                RefreshToken = "refresh_demo_token_masked"
            };
        }
    }

    /// <summary>
    /// Example request for external login.
    /// </summary>
    public sealed class ExternalLoginRequest : SwaggerExampleProvider<ExternalLoginCommand>
    {
        /// <summary>
        /// Builds an external-login request example.
        /// </summary>
        /// <returns>The external-login request example.</returns>
        protected override ExternalLoginCommand BuildExample()
        {
            // ExternalToken is masked because it is issued by the provider to the client app.
            return new ExternalLoginCommand
            {
                Provider = ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE,
                ExternalToken = "external_provider_token_masked"
            };
        }
    }

    /// <summary>
    /// Example request for completing a first-time external registration.
    /// </summary>
    public sealed class CompleteExternalRegistrationRequest : SwaggerExampleProvider<CompleteExternalRegistrationCommand>
    {
        /// <summary>
        /// Builds a complete external-registration request example.
        /// </summary>
        /// <returns>The complete external-registration request example.</returns>
        protected override CompleteExternalRegistrationCommand BuildExample()
        {
            // Keep the provider credential masked while showing every required completion field.
            return new CompleteExternalRegistrationCommand
            {
                Provider = ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE,
                ExternalToken = "external_provider_token_masked",
                FullName = "Nguyen Van A",
                PhoneNumber = AuthenticationSwaggerExampleConstants.EXAMPLE_PHONE_NUMBER,
                PartyType = "TENANT"
            };
        }
    }

    /// <summary>
    /// Example request for forgot-password OTP send.
    /// </summary>
    public sealed class ForgotPasswordRequest : SwaggerExampleProvider<ForgotPasswordCommand>
    {
        /// <summary>
        /// Builds a forgot-password request example.
        /// </summary>
        /// <returns>The forgot-password request example.</returns>
        protected override ForgotPasswordCommand BuildExample()
        {
            // Forgot-password responses remain enumeration-safe even when the email does not exist.
            return new ForgotPasswordCommand
            {
                Email = "tenant@example.com"
            };
        }
    }

    /// <summary>
    /// Example request for forgot-password OTP verification.
    /// </summary>
    public sealed class VerifyForgotPasswordOtpRequest : SwaggerExampleProvider<VerifyForgotPasswordOtpCommand>
    {
        /// <summary>
        /// Builds a forgot-password OTP verification request example.
        /// </summary>
        /// <returns>The forgot-password OTP verification request example.</returns>
        protected override VerifyForgotPasswordOtpCommand BuildExample()
        {
            // OTP is fake and uses the same 4-digit length as the runtime validator.
            return new VerifyForgotPasswordOtpCommand
            {
                Email = "tenant@example.com",
                Otp = "1234"
            };
        }
    }

    /// <summary>
    /// Example request for forgot-password password change.
    /// </summary>
    public sealed class ChangeForgotPasswordRequest : SwaggerExampleProvider<ChangeForgotPasswordCommand>
    {
        /// <summary>
        /// Builds a forgot-password password-change request example.
        /// </summary>
        /// <returns>The forgot-password password-change request example.</returns>
        protected override ChangeForgotPasswordCommand BuildExample()
        {
            // New password values are fake and only show the API password-change shape.
            return new ChangeForgotPasswordCommand
            {
                PasswordResetToken = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                NewPassword = "N3wP@ssw0rd!"
            };
        }
    }

    /// <summary>
    /// Example request for authenticated password change.
    /// </summary>
    public sealed class ChangePasswordRequest : SwaggerExampleProvider<ChangePasswordCommand>
    {
        /// <summary>
        /// Builds an authenticated password-change request example.
        /// </summary>
        /// <returns>The authenticated password-change request example.</returns>
        protected override ChangePasswordCommand BuildExample()
        {
            // Current and new passwords are fake values for contract documentation.
            return new ChangePasswordCommand
            {
                CurrentPassword = "P@ssw0rd!23",
                NewPassword = "N3wP@ssw0rd!"
            };
        }
    }

    /// <summary>
    /// Example request for profile update.
    /// </summary>
    public sealed class UpdateUserInfoRequest : SwaggerExampleProvider<UpdateUserInfoCommand>
    {
        /// <summary>
        /// Builds a user-info update request example.
        /// </summary>
        /// <returns>The user-info update request example.</returns>
        protected override UpdateUserInfoCommand BuildExample()
        {
            // Gender is accepted as the enum string value and avatar file inputs remain manual multipart file pickers.
            return new UpdateUserInfoCommand
            {
                FullName = "Nguyen Van A",
                PhoneNumber = AuthenticationSwaggerExampleConstants.EXAMPLE_PHONE_NUMBER,
                DateOfBirth = new DateOnly(1994, 5, 11),
                Gender = GenderEnum.Male
            };
        }
    }

    /// <summary>
    /// Field-level value examples for profile update.
    /// </summary>
    public sealed class UpdateUserInfoFieldValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds user-info form-field value examples.
        /// </summary>
        /// <returns>The user-info form-field value examples.</returns>
        protected override object BuildExample()
        {
            // File fields are intentionally excluded because Swagger cannot prefill binary file inputs.
            return new
            {
                fullName = AuthenticationSwaggerExampleConstants.EXAMPLE_FULL_NAME_ON_DOCUMENT,
                phoneNumber = AuthenticationSwaggerExampleConstants.EXAMPLE_PHONE_NUMBER,
                dateOfBirth = AuthenticationSwaggerExampleConstants.EXAMPLE_DATE_OF_BIRTH_ON_DOCUMENT,
                gender = AuthenticationSwaggerExampleConstants.EXAMPLE_GENDER
            };
        }
    }

    /// <summary>
    /// Example request for starting change-email verification.
    /// </summary>
    public sealed class ChangeEmailRequest : SwaggerExampleProvider<ChangeEmailCommand>
    {
        /// <summary>
        /// Builds a change-email request example.
        /// </summary>
        /// <returns>The change-email request example.</returns>
        protected override ChangeEmailCommand BuildExample()
        {
            // OTP is sent to the new email while the old email receives a security notice.
            return new ChangeEmailCommand
            {
                NewEmail = "new-email@example.com"
            };
        }
    }

    /// <summary>
    /// Example request for applying a verified email change.
    /// </summary>
    public sealed class VerifyChangeEmailOtpRequest : SwaggerExampleProvider<VerifyChangeEmailOtpCommand>
    {
        /// <summary>
        /// Builds a change-email OTP verification request example.
        /// </summary>
        /// <returns>The change-email OTP verification request example.</returns>
        protected override VerifyChangeEmailOtpCommand BuildExample()
        {
            // OTP is fake and uses the runtime 4-digit OTP contract.
            return new VerifyChangeEmailOtpCommand
            {
                NewEmail = "new-email@example.com",
                Otp = "1234"
            };
        }
    }

    /// <summary>
    /// Example request for identity-document KYC submission.
    /// </summary>
    public sealed class SubmitKycRequest : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds a multipart KYC submission request example.
        /// </summary>
        /// <returns>The KYC multipart request example.</returns>
        protected override object BuildExample()
        {
            // File examples are labels only; clients must send binary multipart file parts.
            return new
            {
                identifierType = AuthenticationSwaggerExampleConstants.EXAMPLE_IDENTIFIER_TYPE,
                identifierValue = AuthenticationSwaggerExampleConstants.EXAMPLE_MASKED_IDENTIFIER,
                fullNameOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_FULL_NAME_ON_DOCUMENT,
                dateOfBirthOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_DATE_OF_BIRTH_ON_DOCUMENT,
                genderOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_GENDER,
                registeredAddress = AuthenticationSwaggerExampleConstants.EXAMPLE_REGISTERED_ADDRESS,
                issuedDate = AuthenticationSwaggerExampleConstants.EXAMPLE_ISSUED_DATE,
                expiredDate = AuthenticationSwaggerExampleConstants.EXAMPLE_EXPIRED_DATE,
                issuedBy = AuthenticationSwaggerExampleConstants.EXAMPLE_ISSUED_BY,
                frontFile = AuthenticationSwaggerExampleConstants.EXAMPLE_FRONT_IDENTITY_DOCUMENT_IMAGE,
                backFile = AuthenticationSwaggerExampleConstants.EXAMPLE_BACK_IDENTITY_DOCUMENT_IMAGE
            };
        }
    }

    /// <summary>
    /// Field-level value examples for identity-document KYC submission.
    /// </summary>
    public sealed class SubmitKycFieldValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds KYC form-field value examples.
        /// </summary>
        /// <returns>The KYC form-field value examples.</returns>
        protected override object BuildExample()
        {
            // File fields are intentionally excluded because Swagger cannot prefill binary file inputs.
            return new
            {
                identifierType = AuthenticationSwaggerExampleConstants.EXAMPLE_IDENTIFIER_TYPE,
                identifierValue = AuthenticationSwaggerExampleConstants.EXAMPLE_MASKED_IDENTIFIER,
                fullNameOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_FULL_NAME_ON_DOCUMENT,
                dateOfBirthOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_DATE_OF_BIRTH_ON_DOCUMENT,
                genderOnDocument = AuthenticationSwaggerExampleConstants.EXAMPLE_GENDER,
                registeredAddress = AuthenticationSwaggerExampleConstants.EXAMPLE_REGISTERED_ADDRESS,
                issuedDate = AuthenticationSwaggerExampleConstants.EXAMPLE_ISSUED_DATE,
                expiredDate = AuthenticationSwaggerExampleConstants.EXAMPLE_EXPIRED_DATE,
                issuedBy = AuthenticationSwaggerExampleConstants.EXAMPLE_ISSUED_BY
            };
        }
    }

    /// <summary>
    /// Example request for linking an external provider.
    /// </summary>
    public sealed class LinkExternalProviderRequest : SwaggerExampleProvider<LinkExternalProviderCommand>
    {
        /// <summary>
        /// Builds an external-provider link request example.
        /// </summary>
        /// <returns>The external-provider link request example.</returns>
        protected override LinkExternalProviderCommand BuildExample()
        {
            // ExternalToken is a masked provider token; the API validates it server-side.
            return new LinkExternalProviderCommand
            {
                Provider = ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE,
                ExternalToken = "external_provider_token_masked"
            };
        }
    }

    /// <summary>
    /// Example request for unlinking an external provider.
    /// </summary>
    public sealed class UnlinkExternalProviderRequest : SwaggerExampleProvider<UnlinkExternalProviderCommand>
    {
        /// <summary>
        /// Builds an external-provider unlink request example.
        /// </summary>
        /// <returns>The external-provider unlink request example.</returns>
        protected override UnlinkExternalProviderCommand BuildExample()
        {
            // The service rejects unlinking the final usable sign-in method.
            return new UnlinkExternalProviderCommand
            {
                Provider = ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE
            };
        }
    }

    /// <summary>
    /// Example request for switching party context.
    /// </summary>
    public sealed class SwitchPartyRequest : SwaggerExampleProvider<SwitchPartyCommand>
    {
        /// <summary>
        /// Builds a switch-party request example.
        /// </summary>
        /// <returns>The switch-party request example.</returns>
        protected override SwitchPartyCommand BuildExample()
        {
            // TargetContext is a party context, not an authorisation role.
            return new SwitchPartyCommand
            {
                TargetContext = PartyTypeEnum.Landlord
            };
        }
    }

    /// <summary>
    /// Example response for token-issuing endpoints.
    /// </summary>
    public sealed class LoginResponse : SwaggerSuccessExampleProvider<LoginResponseDto>
    {
        /// <summary>
        /// Builds a token response example.
        /// </summary>
        /// <returns>The token response example.</returns>
        protected override LoginResponseDto BuildData()
        {
            // Token values are intentionally fake and shortened for documentation safety.
            return new LoginResponseDto
            {
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.demo.signature",
                ExpiresIn = 3600,
                TokenType = "Bearer",
                RefreshToken = "refresh_demo_token_masked"
            };
        }
    }

    /// <summary>
    /// Example response returned after forgot-password OTP verification.
    /// </summary>
    public sealed class PasswordResetTokenResponse : SwaggerSuccessExampleProvider<PasswordResetTokenResponseDto>
    {
        /// <summary>
        /// Builds an opaque password-reset token response example.
        /// </summary>
        /// <returns>The password-reset token response example.</returns>
        protected override PasswordResetTokenResponseDto BuildData()
        {
            // Show reset authority and its short lifetime without exposing a real token.
            return new PasswordResetTokenResponseDto
            {
                PasswordResetToken = "opaque_reset_token_masked",
                ExpiresIn = 300
            };
        }
    }

    /// <summary>
    /// Example authenticated password-change response.
    /// </summary>
    public sealed class ChangePasswordResponse : SwaggerSuccessExampleProvider<ChangePasswordResponseDto>
    {
        /// <summary>
        /// Builds an authenticated password-change response example.
        /// </summary>
        /// <returns>The password-change response example.</returns>
        protected override ChangePasswordResponseDto BuildData()
        {
            // Authenticated password changes return confirmation plus the rotated current-session token pair.
            return new ChangePasswordResponseDto
            {
                Message = "Password changed successfully.",
                Login = BuildLoginData()
            };
        }
    }

    /// <summary>
    /// Example existing-account external-login response.
    /// </summary>
    public sealed class ExternalLoginResponse : SwaggerSuccessExampleProvider<ExternalLoginResponseDto>
    {
        /// <summary>
        /// Builds an existing-account external-login response example.
        /// </summary>
        /// <returns>The external-login response example.</returns>
        protected override ExternalLoginResponseDto BuildData()
        {
            // Existing external identities return only the standard login branch.
            return new ExternalLoginResponseDto
            {
                IsNewRegistration = false,
                Login = BuildLoginData()
            };
        }
    }

    /// <summary>
    /// Example first-time external-login response that requires local registration fields.
    /// </summary>
    public sealed class ExternalRegistrationRequiredResponse : SwaggerSuccessExampleProvider<ExternalLoginResponseDto>
    {
        /// <summary>
        /// Builds a first-time external-login response example.
        /// </summary>
        /// <returns>The registration-required external-login response example.</returns>
        protected override ExternalLoginResponseDto BuildData()
        {
            // First-time identities return only provider-derived prefill data and do not issue Haven tokens yet.
            return new ExternalLoginResponseDto
            {
                IsNewRegistration = true,
                Registration = new ExternalRegistrationPrefillDto
                {
                    Email = "tenant@example.com",
                    FullName = "Nguyen Van A"
                }
            };
        }
    }

    /// <summary>
    /// Example duplicate-refresh conflict response.
    /// </summary>
    public sealed class RefreshDuplicateResponse : SwaggerExampleProvider<ResponseDto<object>>
    {
        /// <summary>
        /// Builds the duplicate-refresh conflict example.
        /// </summary>
        /// <returns>The duplicate-refresh conflict example.</returns>
        protected override ResponseDto<object> BuildExample()
        {
            // Document the stable conflict contract used for an immediate previous refresh hash.
            return new ResponseDto<object>(
                "error_auth_refresh_duplicate",
                "Refresh token request was already processed.",
                StatusCodes.Status409Conflict);
        }
    }

    /// <summary>
    /// Example response for an invalid external provider credential.
    /// </summary>
    public sealed class ExternalUnauthorizedResponse : SwaggerExampleProvider<ResponseDto<object>>
    {
        /// <summary>
        /// Builds the invalid external-credential response example.
        /// </summary>
        /// <returns>The external unauthorised response example.</returns>
        protected override ResponseDto<object> BuildExample()
        {
            // Document invalid or expired provider credentials as unauthorised.
            return new ResponseDto<object>(
                "error_auth_external_provider_invalid",
                "Invalid external token.",
                StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Example response for a conflicting external identity mapping.
    /// </summary>
    public sealed class ExternalConflictResponse : SwaggerExampleProvider<ResponseDto<object>>
    {
        /// <summary>
        /// Builds the external identity conflict response example.
        /// </summary>
        /// <returns>The external identity conflict response example.</returns>
        protected override ResponseDto<object> BuildExample()
        {
            // Document provider ownership conflicts without exposing linked account details.
            return new ResponseDto<object>(
                "error_auth_external_provider_link_conflict",
                "External provider identity is already linked.",
                StatusCodes.Status409Conflict);
        }
    }

    /// <summary>
    /// Builds the shared masked login-token payload used by authentication response examples.
    /// </summary>
    /// <returns>The masked login-token example.</returns>
    private static LoginResponseDto BuildLoginData()
    {
        // Reuse one safe token shape so related Swagger contracts cannot drift independently.
        return new LoginResponseDto
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.demo.signature",
            ExpiresIn = 900,
            TokenType = "Bearer",
            RefreshToken = "refresh_demo_token_masked"
        };
    }

    /// <summary>
    /// Base provider for operation-status examples whose only endpoint-specific value is the success message.
    /// </summary>
    public abstract class OperationStatusResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Gets the exact user-facing message returned by the documented workflow.
        /// </summary>
        protected abstract string Message { get; }

        /// <summary>
        /// Builds an operation-status response example.
        /// </summary>
        /// <returns>The operation-status response example.</returns>
        protected override OperationStatusResponseDto BuildData()
        {
            // Keep the envelope shape shared while requiring each endpoint to document its real business message.
            return new OperationStatusResponseDto
            {
                IsSuccess = true,
                Message = Message
            };
        }
    }

    /// <summary>
    /// Example response for a registration OTP request.
    /// </summary>
    public sealed class RegisterResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.OtpMessages.OTP_SENT_MESSAGE;
    }

    /// <summary>
    /// Example response for completed email registration.
    /// </summary>
    public sealed class VerifyRegisterEmailResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.AccountMessages.REGISTRATION_COMPLETED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for current-session logout.
    /// </summary>
    public sealed class LogoutResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.SessionMessages.LOGOUT_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example enumeration-safe response for a forgot-password request.
    /// </summary>
    public sealed class ForgotPasswordResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.OtpMessages.FORGOT_PASSWORD_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for a completed forgot-password reset.
    /// </summary>
    public sealed class ChangeForgotPasswordResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.PasswordMessages.PASSWORD_CHANGED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for a current-user profile update.
    /// </summary>
    public sealed class UpdateUserInfoResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.ProfileMessages.USER_INFO_UPDATED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for a change-email OTP request.
    /// </summary>
    public sealed class ChangeEmailResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.ProfileMessages.CHANGE_EMAIL_OTP_SENT_MESSAGE;
    }

    /// <summary>
    /// Example response for a completed email change.
    /// </summary>
    public sealed class VerifyChangeEmailOtpResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.ProfileMessages.CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for linking an external provider.
    /// </summary>
    public sealed class LinkExternalProviderResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for unlinking an external provider.
    /// </summary>
    public sealed class UnlinkExternalProviderResponse : OperationStatusResponse
    {
        /// <inheritdoc />
        protected override string Message => ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE;
    }

    /// <summary>
    /// Example response for current-user info.
    /// </summary>
    public sealed class UserInfoResponse : SwaggerSuccessExampleProvider<UserInfoResponseDto>
    {
        /// <summary>
        /// Builds a user-info response example.
        /// </summary>
        /// <returns>The user-info response example.</returns>
        protected override UserInfoResponseDto BuildData()
        {
            // User-info summarizes account, active party context, KYC state, linked providers, and tenant vehicles.
            return new UserInfoResponseDto
            {
                FullName = "Nguyen Van A",
                UserName = "tenant@example.com",
                Email = "tenant@example.com",
                PhoneNumber = AuthenticationSwaggerExampleConstants.EXAMPLE_PHONE_NUMBER,
                AvatarUrl = "https://cdn.example.com/avatars/user-demo.png",
                DateOfBirth = new DateOnly(1994, 5, 11),
                Gender = GenderEnum.Male,
                DisplayName = "Nguyen Van A",
                CurrentContext = PartyTypeEnum.Tenant,
                AvailableContexts = [PartyTypeEnum.Tenant, PartyTypeEnum.Landlord],
                ExternalProviders =
                [
                    new ExternalProviderResponseDto
                    {
                        Provider = ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE,
                        IsLinked = true
                    },
                    new ExternalProviderResponseDto
                    {
                        Provider = ApplicationConstants.EXTERNAL_PROVIDER_FACEBOOK,
                        IsLinked = false
                    }
                ],
                KycSummary = new KycSummaryResponseDto
                {
                    IsSubmitted = true,
                    Status = KycStatusEnum.Pending,
                    IdentifierType = IdentifierTypeEnum.Cccd,
                    IdentifierTypeDisplayName = "CCCD/CMND",
                    MaskedIdentifier = AuthenticationSwaggerExampleConstants.EXAMPLE_MASKED_IDENTIFIER,
                    HasFrontFile = true,
                    HasBackFile = true
                }
            };
        }
    }

    /// <summary>
    /// Example response for KYC submission.
    /// </summary>
    public sealed class KycSubmissionResponse : SwaggerSuccessExampleProvider<KycSubmissionResponseDto>
    {
        /// <summary>
        /// Builds a KYC submission response example.
        /// </summary>
        /// <returns>The KYC submission response example.</returns>
        protected override KycSubmissionResponseDto BuildData()
        {
            // Submit only stages data for manual admin review and does not expose file URLs.
            return new KycSubmissionResponseDto
            {
                IsSubmitted = true,
                Status = KycStatusEnum.Pending,
                Message = "KYC submission has been received for manual review."
            };
        }
    }

    /// <summary>
    /// Example response for party-context switching.
    /// </summary>
    public sealed class SwitchPartyResponse : SwaggerSuccessExampleProvider<SwitchPartyResponseDto>
    {
        /// <summary>
        /// Builds a switch-party response example.
        /// </summary>
        /// <returns>The switch-party response example.</returns>
        protected override SwitchPartyResponseDto BuildData()
        {
            // Switch-party returns the active context and all available contexts after the change.
            return new SwitchPartyResponseDto
            {
                IsSuccess = true,
                Message = "Party context switched successfully.",
                CurrentContext = PartyTypeEnum.Landlord,
                AvailableContexts = [PartyTypeEnum.Tenant, PartyTypeEnum.Landlord]
            };
        }
    }

}

