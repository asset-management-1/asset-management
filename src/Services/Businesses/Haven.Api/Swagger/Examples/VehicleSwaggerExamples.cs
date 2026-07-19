namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups vehicle form, response, route, and query examples.
/// </summary>
internal static class VehicleSwaggerExamples
{
    /// <summary>
    /// Provides vehicle route and query values.
    /// </summary>
    internal sealed class Values : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds safe vehicle route and query examples.
        /// </summary>
        /// <returns>The vehicle route and query values.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, vehicleId = HavenSwaggerExampleConstants.VEHICLE_ID };
    }

    /// <summary>
    /// Provides create-vehicle multipart metadata without binary file values.
    /// </summary>
    internal sealed class CreateValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds create-vehicle metadata fields while leaving file pickers empty.
        /// </summary>
        /// <returns>The create-vehicle metadata fields.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, payerTenantId = HavenSwaggerExampleConstants.TENANT_ID, vehicleTypeCode = "MOTORBIKE", name = "Honda Air Blade", licensePlate = "59A1-12345" };
    }

    /// <summary>
    /// Provides update-vehicle multipart metadata without binary file values.
    /// </summary>
    internal sealed class UpdateValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds update-vehicle metadata fields while leaving file pickers empty.
        /// </summary>
        /// <returns>The update-vehicle metadata fields.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, vehicleId = HavenSwaggerExampleConstants.VEHICLE_ID, payerTenantId = HavenSwaggerExampleConstants.TENANT_ID, vehicleTypeCode = "MOTORBIKE", name = "Honda Air Blade", licensePlate = "59A1-12345" };
    }

    /// <summary>
    /// Provides a vehicle-list success envelope.
    /// </summary>
    internal sealed class ListResponse : SwaggerSuccessExampleProvider<IReadOnlyList<VehicleListItemResponseDto>>
    {
        /// <summary>
        /// Builds a room-scoped vehicle-list response.
        /// </summary>
        /// <returns>The vehicle-list success envelope.</returns>
        protected override IReadOnlyList<VehicleListItemResponseDto> BuildData() =>
        [
            new VehicleListItemResponseDto
            {
                Id = Guid.Parse(HavenSwaggerExampleConstants.VEHICLE_ID),
                Payer = new VehicleListPayerResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.TENANT_ID), Tenant = "Nguyen Van A" },
                VehicleTypeCode = "MOTORBIKE",
                VehicleTypeName = "Motorbike",
                Name = "Honda Air Blade",
                LicensePlate = "59A1-12345"
            }
        ];
    }

    /// <summary>
    /// Provides a vehicle-detail success envelope.
    /// </summary>
    internal sealed class DetailResponse : SwaggerSuccessExampleProvider<VehicleDetailResponseDto>
    {
        /// <summary>
        /// Builds a vehicle-detail response.
        /// </summary>
        /// <returns>The vehicle-detail success envelope.</returns>
        protected override VehicleDetailResponseDto BuildData() => new()
        {
            Id = Guid.Parse(HavenSwaggerExampleConstants.VEHICLE_ID),
            Room = new VehicleRoomResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID), Name = "101" },
            Payer = new VehiclePayerResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.TENANT_ID), Tenant = "Nguyen Van A" },
            VehicleTypeCode = "MOTORBIKE",
            VehicleTypeName = "Motorbike",
            Name = "Honda Air Blade",
            LicensePlate = "59A1-12345"
        };
    }

    /// <summary>
    /// Provides a vehicle-operation success envelope.
    /// </summary>
    internal sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds a successful vehicle deletion response.
        /// </summary>
        /// <returns>The vehicle-operation success envelope.</returns>
        protected override OperationStatusResponseDto BuildData() => new()
        {
            IsSuccess = true,
            Message = "Vehicle deleted successfully."
        };
    }
}
