namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups tenant request, response, route, and query examples.
/// </summary>
internal static class TenantSwaggerExamples
{
    /// <summary>
    /// Provides tenant route and query values.
    /// </summary>
    internal sealed class Values : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds tenant route and query examples.
        /// </summary>
        /// <returns>The tenant route and query values.</returns>
        protected override object BuildExample() => new { id = HavenSwaggerExampleConstants.TENANT_ID, roomPublicId = HavenSwaggerExampleConstants.ROOM_ID, token = HavenSwaggerExampleConstants.TENANT_JOIN_TOKEN, roleCode = "PRIMARY", pageNumber = 1, pageSize = 20 };
    }

    /// <summary>
    /// Provides a landlord-created tenant request.
    /// </summary>
    internal sealed class CreateRequest : SwaggerExampleProvider<CreateTenantCommand>
    {
        /// <summary>
        /// Builds a tenant creation request with inline profile data.
        /// </summary>
        /// <returns>The tenant creation request.</returns>
        protected override CreateTenantCommand BuildExample() => new()
        {
            RoomId = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
            RoleCode = "PRIMARY",
            TenantProfile = new TenantProfileRequestDto { FullName = "Nguyen Van A", Phone = "0901234567", Email = "tenant@example.com" },
            ContractStartDate = new DateTime(2026, 7, 1),
            ContractEndDate = new DateTime(2027, 6, 30),
            ContractRentAmount = 4500000m,
            DepositAmount = 4500000m
        };
    }

    /// <summary>
    /// Provides a tenant join confirmation request.
    /// </summary>
    internal sealed class ConfirmRequest : SwaggerExampleProvider<ConfirmTenantJoinCommand>
    {
        /// <summary>
        /// Builds a tenant join confirmation request with a masked token.
        /// </summary>
        /// <returns>The tenant join confirmation request.</returns>
        protected override ConfirmTenantJoinCommand BuildExample() => new() { Token = HavenSwaggerExampleConstants.TENANT_JOIN_TOKEN };
    }

    /// <summary>
    /// Provides a paged tenant-list success envelope.
    /// </summary>
    internal sealed class ListResponse : SwaggerSuccessExampleProvider<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>
    {
        /// <summary>
        /// Builds a paged tenant-list response.
        /// </summary>
        /// <returns>The tenant-list success envelope.</returns>
        protected override PaginationResponse<IReadOnlyList<TenantListItemResponseDto>> BuildData() => new(
            [BuildTenant()],
            1,
            20,
            1);
    }

    /// <summary>
    /// Provides a tenant-detail success envelope.
    /// </summary>
    internal sealed class DetailResponse : SwaggerSuccessExampleProvider<TenantDetailResponseDto>
    {
        /// <summary>
        /// Builds a tenant occupancy detail response.
        /// </summary>
        /// <returns>The tenant-detail success envelope.</returns>
        protected override TenantDetailResponseDto BuildData()
        {
            var tenant = BuildTenant();
            return new TenantDetailResponseDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Tenant = tenant.Tenant,
                Phone = tenant.Phone,
                Email = "tenant@example.com",
                RoleCode = tenant.RoleCode,
                Property = tenant.Property,
                Room = tenant.Room,
                OccupancyStatusCode = tenant.OccupancyStatusCode,
                OccupancyStatusName = tenant.OccupancyStatusName
            };
        }
    }

    /// <summary>
    /// Provides a tenant-operation success envelope.
    /// </summary>
    internal sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds a successful tenant move-out response.
        /// </summary>
        /// <returns>The tenant-operation success envelope.</returns>
        protected override OperationStatusResponseDto BuildData() => new()
        {
            IsSuccess = true,
            Message = "Tenant moved out successfully."
        };
    }

    /// <summary>
    /// Provides a tenant join QR success envelope.
    /// </summary>
    internal sealed class JoinQrResponse : SwaggerSuccessExampleProvider<TenantJoinQrResponseDto>
    {
        /// <summary>
        /// Builds the short-lived tenant join token response.
        /// </summary>
        /// <returns>The tenant join QR success envelope.</returns>
        protected override TenantJoinQrResponseDto BuildData() => new() { Token = HavenSwaggerExampleConstants.TENANT_JOIN_TOKEN };
    }

    /// <summary>
    /// Provides a tenant join preview success envelope.
    /// </summary>
    internal sealed class JoinPreviewResponse : SwaggerSuccessExampleProvider<TenantJoinPreviewResponseDto>
    {
        /// <summary>
        /// Builds the non-mutating room join preview response.
        /// </summary>
        /// <returns>The tenant join preview success envelope.</returns>
        protected override TenantJoinPreviewResponseDto BuildData() => new()
        {
            Property = new TenantPropertySummaryResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID), Name = "Haven Nguyen Hue" },
            Room = new TenantJoinRoomSummaryResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID), Name = "101" }
        };
    }

    /// <summary>
    /// Builds one documented tenant occupancy.
    /// </summary>
    /// <returns>The documented tenant payload.</returns>
    private static TenantListItemResponseDto BuildTenant() => new()
    {
        Id = Guid.Parse(HavenSwaggerExampleConstants.TENANT_ID),
        TenantId = Guid.Parse(HavenSwaggerExampleConstants.TENANT_ID),
        Tenant = "Nguyen Van A",
        Phone = "0901234567",
        RoleCode = "PRIMARY",
        Property = new TenantPropertySummaryResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID), Name = "Haven Nguyen Hue" },
        Room = new TenantRoomSummaryResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID), Code = "ROOM-101", Name = "101" },
        OccupancyStatusCode = "ACTIVE",
        OccupancyStatusName = "Active"
    };
}
