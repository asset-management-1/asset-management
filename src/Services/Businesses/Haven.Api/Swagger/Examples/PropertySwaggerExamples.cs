namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups property request, response, route, and query examples.
/// </summary>
internal static class PropertySwaggerExamples
{
    /// <summary>
    /// Provides a property list query example.
    /// </summary>
    internal sealed class ListValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds property list filter and paging examples.
        /// </summary>
        /// <returns>The property list query values.</returns>
        protected override object BuildExample() => new { pageNumber = 1, pageSize = 20, searchText = "Nguyen Hue", includeStructure = true };
    }

    /// <summary>
    /// Provides a property route identifier example.
    /// </summary>
    internal sealed class RouteValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds the public property route identifier example.
        /// </summary>
        /// <returns>The property route value.</returns>
        protected override object BuildExample() => new { propertyPublicId = HavenSwaggerExampleConstants.PROPERTY_ID };
    }

    /// <summary>
    /// Provides a create-property body example.
    /// </summary>
    internal sealed class CreateRequest : SwaggerExampleProvider<CreatePropertyCommand>
    {
        /// <summary>
        /// Builds a complete property creation request.
        /// </summary>
        /// <returns>The property creation example.</returns>
        protected override CreatePropertyCommand BuildExample() => new()
        {
            Name = "Haven Nguyen Hue",
            PropertyTypeCode = "APARTMENT",
            ProvinceCode = "79",
            DistrictCode = "760",
            WardCode = "26734",
            StreetAddress = "12 Nguyen Hue",
            FormattedAddress = "12 Nguyen Hue, Ho Chi Minh City",
            Latitude = 10.773m,
            Longitude = 106.704m,
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Name = "101",
                                TypeCode = "STUDIO",
                                RentalModeCode = "WHOLE_ROOM",
                                BaseRentAmount = 4500000m,
                                DefaultDepositAmount = 4500000m
                            }
                        ]
                    }
                ]
            }
        };
    }

    /// <summary>
    /// Provides an update-property body example.
    /// </summary>
    internal sealed class UpdateRequest : SwaggerExampleProvider<UpdatePropertyCommand>
    {
        /// <summary>
        /// Builds an editable property request.
        /// </summary>
        /// <returns>The property update example.</returns>
        protected override UpdatePropertyCommand BuildExample() => new()
        {
            Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID),
            Name = "Haven Nguyen Hue",
            PropertyTypeCode = "APARTMENT",
            ProvinceCode = "79",
            DistrictCode = "760",
            WardCode = "26734",
            StreetAddress = "12 Nguyen Hue",
            FormattedAddress = "12 Nguyen Hue, Ho Chi Minh City",
            Latitude = 10.773m,
            Longitude = 106.704m
        };
    }

    /// <summary>
    /// Provides a paged property success envelope.
    /// </summary>
    internal sealed class ListResponse : SwaggerSuccessExampleProvider<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>
    {
        /// <summary>
        /// Builds the paged property list success envelope.
        /// </summary>
        /// <returns>The property list response.</returns>
        protected override PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>> BuildData() => new(
            [new PropertyListItemResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID), Name = "Haven Nguyen Hue", Address = "12 Nguyen Hue, Ho Chi Minh City", TotalFloors = 1, TotalRooms = 1, AvailableRooms = 1 }],
            1,
            20,
            1);
    }

    /// <summary>
    /// Provides a property detail success envelope.
    /// </summary>
    internal sealed class DetailResponse : SwaggerSuccessExampleProvider<PropertyDetailResponseDto>
    {
        /// <summary>
        /// Builds the property detail success envelope.
        /// </summary>
        /// <returns>The property detail response.</returns>
        protected override PropertyDetailResponseDto BuildData() => new()
        {
            BasicInfo = new PropertyDetailBasicInfoResponseDto
            {
                Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID),
                Code = "PROPERTY-001",
                Name = "Haven Nguyen Hue",
                PropertyTypeCode = "APARTMENT",
                FormattedAddress = "12 Nguyen Hue, Ho Chi Minh City"
            }
        };
    }

    /// <summary>
    /// Provides a created-property success envelope.
    /// </summary>
    internal sealed class CreateResponse : SwaggerSuccessExampleProvider<CreatedPropertyResponseDto>
    {
        /// <summary>
        /// Builds the created-property success envelope.
        /// </summary>
        /// <returns>The property creation response.</returns>
        protected override CreatedPropertyResponseDto BuildData() => new()
        {
            Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID),
            PropertyCode = "PROPERTY-001",
            Name = "Haven Nguyen Hue",
            TotalFloors = 1,
            TotalRooms = 1
        };
    }

    /// <summary>
    /// Provides a property operation success envelope.
    /// </summary>
    internal sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds a successful property mutation status envelope.
        /// </summary>
        /// <returns>The property operation response.</returns>
        protected override OperationStatusResponseDto BuildData() => new()
        {
            IsSuccess = true,
            Message = "Property operation completed successfully."
        };
    }
}
