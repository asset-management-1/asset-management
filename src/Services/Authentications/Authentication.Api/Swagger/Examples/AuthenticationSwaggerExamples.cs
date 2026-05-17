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
            // PartyType is an enum in the API contract and is normalized by Application mapping after validation.
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
                Provider = "google",
                ExternalToken = "external_provider_token_masked"
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
                Email = "tenant@example.com",
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
                frontFile = "(binary front or primary identity document image)",
                backFile = "(binary back identity document image, required for CCCD)"
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
    /// Example request for vehicle registration.
    /// </summary>
    public sealed class RegisterVehicleRequest : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds a multipart vehicle-registration request example.
        /// </summary>
        /// <returns>The vehicle-registration multipart request example.</returns>
        protected override object BuildExample()
        {
            // Vehicle images are optional binary multipart file parts.
            return new
            {
                vehicleType = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_TYPE,
                vehicleName = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_NAME,
                licensePlate = AuthenticationSwaggerExampleConstants.EXAMPLE_LICENSE_PLATE,
                frontFile = "(binary front vehicle image)",
                sideFile = "(binary side vehicle image)"
            };
        }
    }

    /// <summary>
    /// Field-level value examples for vehicle registration.
    /// </summary>
    public sealed class RegisterVehicleFieldValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds vehicle-registration form-field value examples.
        /// </summary>
        /// <returns>The vehicle-registration form-field value examples.</returns>
        protected override object BuildExample()
        {
            // File fields are intentionally excluded because Swagger cannot prefill binary file inputs.
            return new
            {
                vehicleType = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_TYPE,
                vehicleName = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_NAME,
                licensePlate = AuthenticationSwaggerExampleConstants.EXAMPLE_LICENSE_PLATE
            };
        }
    }

    /// <summary>
    /// Field-level value example for a vehicle public identifier route parameter.
    /// </summary>
    public sealed class VehiclePublicIdValue : SwaggerExampleProvider<string>
    {
        /// <summary>
        /// Builds the vehicle public identifier value example.
        /// </summary>
        /// <returns>The fake vehicle public identifier.</returns>
        protected override string BuildExample()
        {
            // The value is fake documentation data and does not identify a real tenant vehicle.
            return AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_PUBLIC_ID;
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
                Provider = "google",
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
                Provider = "google"
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
            // TargetContext is a party context, not an authorization role.
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
    /// Example response for operation-status endpoints.
    /// </summary>
    public sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds an operation-status response example.
        /// </summary>
        /// <returns>The operation-status response example.</returns>
        protected override OperationStatusResponseDto BuildData()
        {
            // Generic operation responses only carry success state and a user-facing message.
            return new OperationStatusResponseDto
            {
                IsSuccess = true,
                Message = "Operation completed successfully."
            };
        }
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
                        Provider = "google",
                        IsLinked = true
                    },
                    new ExternalProviderResponseDto
                    {
                        Provider = "apple",
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
                },
                RegisteredVehicles =
                [
                    BuildVehicle()
                ]
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
    /// Example response for a vehicle item.
    /// </summary>
    public sealed class VehicleResponse : SwaggerSuccessExampleProvider<UserVehicleResponseDto>
    {
        /// <summary>
        /// Builds a vehicle response example.
        /// </summary>
        /// <returns>The vehicle response example.</returns>
        protected override UserVehicleResponseDto BuildData()
        {
            // Vehicle images are non-sensitive profile images and may be returned for UI thumbnails.
            return BuildVehicle();
        }
    }

    /// <summary>
    /// Example response for a vehicle list.
    /// </summary>
    public sealed class VehicleListResponse : SwaggerSuccessExampleProvider<IReadOnlyList<UserVehicleResponseDto>>
    {
        /// <summary>
        /// Builds a vehicle-list response example.
        /// </summary>
        /// <returns>The vehicle-list response example.</returns>
        protected override IReadOnlyList<UserVehicleResponseDto> BuildData()
        {
            // List response contains active vehicles under the current tenant party only.
            return
            [
                BuildVehicle()
            ];
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

    /// <summary>
    /// Builds the shared vehicle example used by profile and vehicle endpoints.
    /// </summary>
    /// <returns>The vehicle response example.</returns>
    private static UserVehicleResponseDto BuildVehicle()
    {
        // Keep vehicle examples focused on current profile fields and out of billing/parking concerns.
        return new UserVehicleResponseDto
        {
            PublicId = Guid.Parse(AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_PUBLIC_ID),
            VehicleType = VehicleTypeEnum.Motorbike,
            VehicleTypeDisplayName = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_TYPE,
            VehicleName = AuthenticationSwaggerExampleConstants.EXAMPLE_VEHICLE_NAME,
            LicensePlate = AuthenticationSwaggerExampleConstants.EXAMPLE_LICENSE_PLATE,
            Status = UserVehicleStatusEnum.Active,
            ThumbnailUrl = "https://cdn.example.com/vehicles/front-demo.png",
            FrontImageUrl = "https://cdn.example.com/vehicles/front-demo.png",
            SideImageUrl = "https://cdn.example.com/vehicles/side-demo.png"
        };
    }
}

