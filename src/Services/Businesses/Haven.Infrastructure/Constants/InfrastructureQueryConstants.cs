namespace Haven.Infrastructure.Constants;

/// <summary>
/// Stores SQL text for Haven business optimized reads and lookup operations.
/// </summary>
public static class InfrastructureQueryConstants
{
    /// <summary>
    /// Locks one active room row before package materialization and mutation.
    /// </summary>
    public const string GET_ROOM_ID_FOR_PACKAGE_MUTATION_QUERY = """
        SELECT unit."Id" AS "Value"
        FROM "asset"."Units" unit
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
           ON property_party."PropertyId" = property."Id"
           AND property_party."PartyId" = @CurrentPartyId
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        WHERE unit."PublicId" = @RoomPublicId
          AND unit."IsDeleted" = FALSE
        FOR UPDATE;
        """;

    /// <summary>
    /// Loads every active vehicle for a landlord-scoped room.
    /// </summary>
    public const string GET_VEHICLE_LIST_QUERY = """
        SELECT
            vehicle."PublicId" AS "VehiclePublicId",
            payer."PublicId" AS "PayerTenantPublicId",
            payer."DisplayName" AS "PayerTenantName",
            vehicle_type."Code" AS "VehicleTypeCode",
            vehicle_type."Name" AS "VehicleTypeName",
            vehicle."VehicleName",
            vehicle."LicensePlate"
        FROM "core"."PartyVehicles" vehicle
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = vehicle."UnitId"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
           ON property_party."PropertyId" = property."Id"
           AND property_party."PartyId" = @CurrentPartyId
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        INNER JOIN "core"."Parties" payer
            ON payer."Id" = vehicle."PartyId"
           AND payer."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" vehicle_type
            ON vehicle_type."Id" = vehicle."VehicleTypeId"
           AND vehicle_type."IsDeleted" = FALSE
           AND vehicle_type."IsActive" = TRUE
        WHERE vehicle."IsDeleted" = FALSE
          AND unit."PublicId" = @RoomId
        ORDER BY vehicle."CreatedAt" DESC, vehicle."Id" DESC;
        """;

    /// <summary>
    /// Loads one vehicle registration detail inside the current landlord scope.
    /// </summary>
    public const string GET_VEHICLE_DETAIL_QUERY = """
        SELECT
            vehicle."PublicId" AS "VehiclePublicId",
            unit."PublicId" AS "RoomPublicId",
            unit."UnitName" AS "RoomName",
            payer."PublicId" AS "PayerTenantPublicId",
            payer."DisplayName" AS "PayerTenantName",
            vehicle_type."Code" AS "VehicleTypeCode",
            vehicle_type."Name" AS "VehicleTypeName",
            vehicle."VehicleName",
            vehicle."LicensePlate",
            vehicle."RegistrationFrontImageUrl",
            vehicle."RegistrationSideImageUrl",
            vehicle."VehicleFrontImageUrl",
            vehicle."VehicleSideImageUrl"
        FROM "core"."PartyVehicles" vehicle
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = vehicle."UnitId"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "core"."Parties" payer
            ON payer."Id" = vehicle."PartyId"
           AND payer."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" vehicle_type
            ON vehicle_type."Id" = vehicle."VehicleTypeId"
           AND vehicle_type."IsDeleted" = FALSE
           AND vehicle_type."IsActive" = TRUE
        WHERE vehicle."PublicId" = @VehiclePublicId
          AND unit."PublicId" = @RoomPublicId
          AND vehicle."IsDeleted" = FALSE
          AND EXISTS (
                SELECT 1
                 FROM "asset"."PropertyParties" property_party
                 INNER JOIN "masterdata"."MasterDataValues" relationship_type
                     ON relationship_type."Id" = property_party."RelationshipTypeId"
                    AND relationship_type."IsDeleted" = FALSE
                    AND relationship_type."IsActive" = TRUE
                    AND relationship_type."Code" = ANY(@RelationshipCodes)
                 WHERE property_party."PropertyId" = unit."PropertyId"
                   AND property_party."PartyId" = @CurrentPartyId
                   AND property_party."IsDeleted" = FALSE
                   AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                   AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
           )
        LIMIT 1;
        """;

    /// <summary>
    /// Loads one tracked vehicle only when its property is visible to the current landlord relationship scope.
    /// </summary>
    public const string GET_SCOPED_VEHICLE_ID_QUERY = """
        SELECT vehicle."Id" AS "Value"
        FROM "core"."PartyVehicles" vehicle
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = vehicle."UnitId"
           AND unit."IsDeleted" = FALSE
        WHERE vehicle."PublicId" = @VehiclePublicId
          AND unit."PublicId" = @RoomPublicId
          AND vehicle."IsDeleted" = FALSE
          AND EXISTS (
                SELECT 1
                FROM "asset"."Properties" property
                INNER JOIN "asset"."PropertyParties" property_party
                    ON property_party."PropertyId" = property."Id"
                   AND property_party."PartyId" = @CurrentPartyId
                   AND property_party."IsDeleted" = FALSE
                   AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                   AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
                INNER JOIN "masterdata"."MasterDataValues" relationship_type
                    ON relationship_type."Id" = property_party."RelationshipTypeId"
                   AND relationship_type."IsDeleted" = FALSE
                   AND relationship_type."IsActive" = TRUE
                   AND relationship_type."Code" = ANY(@RelationshipCodes)
                WHERE property."Id" = unit."PropertyId"
                  AND property."IsDeleted" = FALSE
          )
        LIMIT 1;
        """;

    /// <summary>
    /// Resolves an active contract-signing primary tenant eligible to pay for a vehicle.
    /// </summary>
    public const string GET_VEHICLE_PRIMARY_PAYER_QUERY = """
        SELECT
            unit."Id" AS "UnitId",
            payer."Id" AS "PartyId"
        FROM "leasing"."Occupancies" occupancy
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = occupancy."UnitId"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "core"."Parties" payer
            ON payer."Id" = occupancy."PartyId"
           AND payer."IsDeleted" = FALSE
        INNER JOIN "leasing"."Contracts" contract
            ON contract."Id" = occupancy."ContractId"
           AND contract."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."PartyId" = @CurrentPartyId
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        WHERE payer."PublicId" = @PayerTenantPublicId
          AND occupancy."IsDeleted" = FALSE
          AND occupancy."IsPrimaryTenant" = TRUE
          AND occupancy."StatusId" = @ActiveOccupancyStatusId
          AND contract."StatusId" = @ActiveContractStatusId
          AND contract."UnitId" = unit."Id"
          AND contract."SecondaryPartyId" = payer."Id"
          AND contract."EffectiveFrom" <= CURRENT_DATE
          AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
          AND (@RoomPublicId IS NULL OR unit."PublicId" = @RoomPublicId)
          AND (@UnitId IS NULL OR unit."Id" = @UnitId)
        LIMIT 1;
        """;

    /// <summary>
    /// Loads one room row under an active property-party relationship for tracked EF graph composition.
    /// </summary>
    public const string GET_SCOPED_ROOM_ID_QUERY = """
        SELECT unit."Id" AS "Value"
        FROM "asset"."Units" unit
        WHERE unit."PublicId" = @RoomPublicId
          AND unit."IsDeleted" = FALSE
          AND EXISTS (
                SELECT 1
                FROM "asset"."Properties" property
                INNER JOIN "asset"."PropertyParties" property_party
                    ON property_party."PropertyId" = property."Id"
                   AND property_party."IsDeleted" = FALSE
                   AND property_party."PartyId" = @CurrentPartyId
                   AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                   AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
                INNER JOIN "masterdata"."MasterDataValues" relationship_type
                    ON relationship_type."Id" = property_party."RelationshipTypeId"
                   AND relationship_type."IsDeleted" = FALSE
                   AND relationship_type."IsActive" = TRUE
                   AND relationship_type."Code" = ANY(@RelationshipCodes)
                WHERE property."Id" = unit."PropertyId"
                  AND property."IsDeleted" = FALSE
          )
        """;

    /// <summary>
    /// Resolves active master-data values by type/code pairs.
    /// </summary>
    public const string GET_MASTER_DATA_VALUES_BY_TYPE_AND_CODE_QUERY = """
        WITH requested AS (
            SELECT
                request."Ordinal",
                request."Type" AS "KeyType",
                request."Code" AS "KeyCode"
            FROM UNNEST(@KeyTypes, @KeyCodes, @KeyOrdinals) AS request("Type", "Code", "Ordinal")
            WHERE request."Type" IS NOT NULL
              AND request."Type" <> ''
              AND request."Code" IS NOT NULL
              AND request."Code" <> ''
        )
        SELECT DISTINCT ON (requested."Ordinal")
            requested."KeyType",
            requested."KeyCode",
            master_value."Id",
            master_type."Code" AS "Type",
            master_value."Code",
            master_value."Name",
            master_value."Description"
        FROM requested
        INNER JOIN "masterdata"."MasterDataTypes" master_type
            ON master_type."IsDeleted" = FALSE
           AND master_type."Code" = requested."KeyType"
        INNER JOIN "masterdata"."MasterDataValues" master_value
            ON master_value."MasterDataTypeId" = master_type."Id"
           AND master_value."IsDeleted" = FALSE
           AND master_value."IsActive" = TRUE
           AND master_value."Code" = requested."KeyCode"
        ORDER BY
            requested."Ordinal",
            master_value."Id";
        """;

    /// <summary>
    /// Loads the active Party selected by the current refresh-token client session.
    /// </summary>
    public const string GET_CURRENT_PARTY_CONTEXT_QUERY = """
        SELECT
            party."Id" AS "PartyId",
            party."PublicId" AS "PartyPublicId",
            party."DisplayName",
            party_type."Code" AS "PartyTypeCode",
            party_status."Code" AS "StatusCode"
        FROM "identity"."RefreshTokens" auth_session
        INNER JOIN "identity"."Users" "user"
            ON "user"."Id" = auth_session."UserId"
           AND "user"."IsDeleted" = FALSE
        INNER JOIN "identity"."UserParties" user_party
            ON user_party."UserId" = auth_session."UserId"
           AND user_party."PartyId" = auth_session."CurrentPartyId"
           AND user_party."IsDeleted" = FALSE
        INNER JOIN "core"."Parties" party
            ON party."Id" = user_party."PartyId"
           AND party."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" party_type
            ON party_type."Id" = party."PartyTypeId"
           AND party_type."IsDeleted" = FALSE
           AND party_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" party_status
            ON party_status."Id" = party."StatusId"
           AND party_status."IsDeleted" = FALSE
           AND party_status."IsActive" = TRUE
        WHERE auth_session."SessionPublicId" = @SessionPublicId
          AND auth_session."RevokedAt" IS NULL
          AND auth_session."ExpiresAt" > NOW()
          AND auth_session."IsDeleted" = FALSE
          AND "user"."PublicId" = @UserPublicId
        LIMIT 1;
        """;

    /// <summary>
    /// Gets a province by code.
    /// </summary>
    public const string GET_PROVINCE_BY_CODE_QUERY = """
        SELECT "Id", "Code", "Name"
        FROM "core"."Provinces"
        WHERE "Code" = @Code
          AND "IsDeleted" = FALSE
        LIMIT 1;
        """;

    /// <summary>
    /// Gets a district by code and optional province.
    /// </summary>
    public const string GET_DISTRICT_BY_CODE_QUERY = """
        SELECT "Id", "Code", "Name"
        FROM "core"."Districts"
        WHERE "Code" = @Code
          AND (@ProvinceId IS NULL OR "ProvinceId" = @ProvinceId)
          AND "IsDeleted" = FALSE
        LIMIT 1;
        """;

    /// <summary>
    /// Gets a ward by code and optional district.
    /// </summary>
    public const string GET_WARD_BY_CODE_QUERY = """
        SELECT "Id", "Code", "Name"
        FROM "core"."Wards"
        WHERE "Code" = @Code
          AND (@DistrictId IS NULL OR "DistrictId" = @DistrictId)
          AND "IsDeleted" = FALSE
        LIMIT 1;
        """;

    /// <summary>
    /// Loads one page of landlord property cards with total count metadata.
    /// </summary>
    public const string GET_PROPERTY_LIST_PAGE_QUERY = """
        WITH scoped_properties AS (
            SELECT property."Id"
            FROM "asset"."Properties" property
            WHERE property."IsDeleted" = FALSE
              AND EXISTS (
                    SELECT 1
                    FROM "asset"."PropertyParties" property_party
                    INNER JOIN "masterdata"."MasterDataValues" relationship_type
                        ON relationship_type."Id" = property_party."RelationshipTypeId"
                       AND relationship_type."IsDeleted" = FALSE
                       AND relationship_type."IsActive" = TRUE
                       AND relationship_type."Code" = ANY(@RelationshipCodes)
                    WHERE property_party."PropertyId" = property."Id"
                      AND property_party."IsDeleted" = FALSE
                      AND property_party."PartyId" = @CurrentPartyId
                      AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                      AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
              )
        ),
        filtered_properties AS (
            SELECT
                property."Id",
                CASE
                    WHEN @Search IS NULL THEN 0
                    WHEN property."PropertyCode" ILIKE @Search || '%' THEN 0
                    WHEN EXISTS (
                        SELECT 1
                        FROM "asset"."Units" search_unit
                        WHERE search_unit."PropertyId" = property."Id"
                          AND search_unit."IsDeleted" = FALSE
                          AND search_unit."UnitCode" ILIKE @Search || '%'
                    ) THEN 1
                    WHEN property."Name" ILIKE '%' || @Search || '%' THEN 2
                    WHEN property."FormattedAddress" ILIKE '%' || @Search || '%' THEN 3
                    ELSE 4
                END AS "SearchRank"
            FROM scoped_properties scoped
            INNER JOIN "asset"."Properties" property
                ON property."Id" = scoped."Id"
               AND property."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" property_type
                ON property_type."Id" = property."PropertyTypeId"
               AND property_type."IsDeleted" = FALSE
               AND property_type."IsActive" = TRUE
            INNER JOIN "masterdata"."MasterDataValues" property_status
                ON property_status."Id" = property."StatusId"
               AND property_status."IsDeleted" = FALSE
               AND property_status."IsActive" = TRUE
            WHERE (@PropertyTypeCode IS NULL OR property_type."Code" = @PropertyTypeCode)
              AND (@StatusCode IS NULL OR property_status."Code" = @StatusCode)
              AND (
                    @Search IS NULL
                    OR property."PropertyCode" ILIKE @Search || '%'
                    OR property."Name" ILIKE '%' || @Search || '%'
                    OR property."FormattedAddress" ILIKE '%' || @Search || '%'
                    OR EXISTS (
                        SELECT 1
                        FROM "asset"."Units" search_unit
                        WHERE search_unit."PropertyId" = property."Id"
                          AND search_unit."IsDeleted" = FALSE
                          AND (
                                search_unit."UnitCode" ILIKE @Search || '%'
                                OR search_unit."UnitName" ILIKE '%' || @Search || '%'
                              )
                    )
              )
              AND (
                    @FloorNumber IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM "asset"."Units" floor_unit
                        WHERE floor_unit."PropertyId" = property."Id"
                          AND floor_unit."FloorNumber" = @FloorNumber
                          AND floor_unit."IsDeleted" = FALSE
                    )
              )
              AND (
                    @RoomStatusCode IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM "asset"."Units" status_unit
                        INNER JOIN "masterdata"."MasterDataValues" unit_status
                            ON unit_status."Id" = status_unit."StatusId"
                           AND unit_status."IsDeleted" = FALSE
                           AND unit_status."IsActive" = TRUE
                        WHERE status_unit."PropertyId" = property."Id"
                          AND status_unit."IsDeleted" = FALSE
                          AND unit_status."Code" = @RoomStatusCode
                    )
              )
              AND (
                    @PaymentStatusCode IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM "asset"."Units" payment_unit
                        LEFT JOIN LATERAL (
                            SELECT invoice_status."Code"
                            FROM "leasing"."Contracts" contract
                            INNER JOIN "masterdata"."MasterDataValues" contract_status
                                ON contract_status."Id" = contract."StatusId"
                               AND contract_status."IsDeleted" = FALSE
                               AND contract_status."IsActive" = TRUE
                               AND contract_status."Code" = 'ACTIVE'
                            INNER JOIN "billing"."Invoices" invoice
                                ON invoice."ContractId" = contract."Id"
                               AND invoice."IsDeleted" = FALSE
                            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                                ON invoice_status."Id" = invoice."StatusId"
                               AND invoice_status."IsDeleted" = FALSE
                               AND invoice_status."IsActive" = TRUE
                            WHERE contract."UnitId" = payment_unit."Id"
                              AND contract."IsDeleted" = FALSE
                              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
                              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
                            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
                            LIMIT 1
                        ) latest_invoice ON TRUE
                        WHERE payment_unit."PropertyId" = property."Id"
                          AND payment_unit."IsDeleted" = FALSE
                          AND latest_invoice."Code" = @PaymentStatusCode
                    )
              )
        ),
        paged_properties AS (
            SELECT
                filtered."Id",
                filtered."SearchRank",
                COUNT(*) OVER()::int AS "TotalCount"
            FROM filtered_properties filtered
            INNER JOIN "asset"."Properties" property
                ON property."Id" = filtered."Id"
               AND property."IsDeleted" = FALSE
            ORDER BY filtered."SearchRank", property."UpdatedAt" DESC NULLS LAST, property."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
        )
        SELECT
            paged."TotalCount",
            property."Id" AS "PropertyId",
            property."PublicId" AS "PropertyPublicId",
            property."PropertyCode",
            property."Name",
            property_type."Code" AS "PropertyTypeCode",
            property_type."Name" AS "PropertyTypeName",
            property_status."Code" AS "StatusCode",
            property_status."Name" AS "StatusName",
            property."IsPublished",
            province."Code" AS "ProvinceCode",
            province."Name" AS "ProvinceName",
            district."Code" AS "DistrictCode",
            district."Name" AS "DistrictName",
            ward."Code" AS "WardCode",
            ward."Name" AS "WardName",
            property."StreetAddress",
            property."FormattedAddress",
            property."Latitude",
            property."Longitude",
            COALESCE(property."TotalFloors", 0)::int AS "TotalFloors",
            COALESCE(property."TotalUnits", 0)::int AS "TotalUnits",
            COALESCE(summary."AvailableUnitCount", 0)::int AS "AvailableUnitCount",
            COALESCE(summary."OccupiedUnitCount", 0)::int AS "OccupiedUnitCount",
            CASE
                WHEN COALESCE(summary."TotalUnits", 0) = 0 THEN NULL
                ELSE COALESCE(summary."OccupiedUnitCount", 0)::decimal / summary."TotalUnits"
            END AS "OccupancyRate",
            COALESCE(summary."MaintenanceUnitCount", 0)::int AS "MaintenanceUnitCount",
            COALESCE(summary."PublishedUnitCount", 0)::int AS "PublishedUnitCount"
        FROM paged_properties paged
        INNER JOIN "asset"."Properties" property
            ON property."Id" = paged."Id"
           AND property."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" property_type
            ON property_type."Id" = property."PropertyTypeId"
           AND property_type."IsDeleted" = FALSE
           AND property_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" property_status
            ON property_status."Id" = property."StatusId"
           AND property_status."IsDeleted" = FALSE
           AND property_status."IsActive" = TRUE
        LEFT JOIN "core"."Provinces" province
            ON province."Id" = property."ProvinceId"
           AND province."IsDeleted" = FALSE
        LEFT JOIN "core"."Districts" district
            ON district."Id" = property."DistrictId"
           AND district."IsDeleted" = FALSE
        LEFT JOIN "core"."Wards" ward
            ON ward."Id" = property."WardId"
           AND ward."IsDeleted" = FALSE
        LEFT JOIN LATERAL (
            SELECT
                COUNT(unit."Id")::int AS "TotalUnits",
                COUNT(DISTINCT unit."FloorNumber") FILTER (WHERE unit."FloorNumber" IS NOT NULL)::int AS "TotalFloors",
                COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'AVAILABLE')::int AS "AvailableUnitCount",
                COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'OCCUPIED')::int AS "OccupiedUnitCount",
                COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'MAINTENANCE')::int AS "MaintenanceUnitCount",
                COUNT(unit."Id") FILTER (WHERE unit."IsPublished" = TRUE)::int AS "PublishedUnitCount"
            FROM "asset"."Units" unit
            INNER JOIN "masterdata"."MasterDataValues" unit_status
                ON unit_status."Id" = unit."StatusId"
               AND unit_status."IsDeleted" = FALSE
               AND unit_status."IsActive" = TRUE
            WHERE unit."PropertyId" = property."Id"
              AND unit."IsDeleted" = FALSE
        ) summary ON TRUE
        ORDER BY paged."SearchRank", property."UpdatedAt" DESC NULLS LAST, property."CreatedAt" DESC;
        """;

    /// <summary>
    /// Loads one property card/detail header scoped to the current party.
    /// </summary>
    public const string GET_PROPERTY_DETAIL_HEADER_QUERY = """
        SELECT
            property."Id" AS "PropertyId",
            property."PublicId" AS "PropertyPublicId",
            property."PropertyCode",
            property."Name",
            property."Description",
            property_type."Code" AS "PropertyTypeCode",
            property_type."Name" AS "PropertyTypeName",
            property_status."Code" AS "StatusCode",
            property_status."Name" AS "StatusName",
            province."Code" AS "ProvinceCode",
            province."Name" AS "ProvinceName",
            district."Code" AS "DistrictCode",
            district."Name" AS "DistrictName",
            ward."Code" AS "WardCode",
            ward."Name" AS "WardName",
            property."StreetAddress",
            property."FormattedAddress",
            property."Latitude",
            property."Longitude",
            COALESCE(property."TotalFloors", 0)::int AS "TotalFloors",
            COALESCE(property."TotalUnits", 0)::int AS "TotalUnits"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."IsDeleted" = FALSE
           AND property_party."PartyId" = @CurrentPartyId
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        INNER JOIN "masterdata"."MasterDataValues" property_type
            ON property_type."Id" = property."PropertyTypeId"
           AND property_type."IsDeleted" = FALSE
           AND property_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" property_status
            ON property_status."Id" = property."StatusId"
           AND property_status."IsDeleted" = FALSE
           AND property_status."IsActive" = TRUE
        LEFT JOIN "core"."Provinces" province
            ON province."Id" = property."ProvinceId"
           AND province."IsDeleted" = FALSE
        LEFT JOIN "core"."Districts" district
            ON district."Id" = property."DistrictId"
           AND district."IsDeleted" = FALSE
        LEFT JOIN "core"."Wards" ward
            ON ward."Id" = property."WardId"
           AND ward."IsDeleted" = FALSE
        WHERE property."IsDeleted" = FALSE
          AND property."PublicId" = @PropertyPublicId
        ORDER BY property."UpdatedAt" DESC NULLS LAST, property."CreatedAt" DESC
        LIMIT 1;
        """;

    /// <summary>
    /// Loads floor summaries for selected properties.
    /// </summary>
    public const string GET_PROPERTY_FLOORS_BY_PUBLIC_IDS_QUERY = """
        SELECT
            property."PublicId" AS "PropertyPublicId",
            unit."FloorNumber",
            COUNT(unit."Id")::int AS "UnitCount",
            COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'AVAILABLE')::int AS "AvailableUnitCount",
            COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'OCCUPIED')::int AS "OccupiedUnitCount",
            COUNT(unit."Id") FILTER (WHERE unit_status."Code" = 'MAINTENANCE')::int AS "MaintenanceUnitCount"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."Units" unit
            ON unit."PropertyId" = property."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" unit_status
            ON unit_status."Id" = unit."StatusId"
           AND unit_status."IsDeleted" = FALSE
           AND unit_status."IsActive" = TRUE
        LEFT JOIN LATERAL (
            SELECT
                invoice_status."Code" AS "PaymentStatusCode"
            FROM "leasing"."Contracts" contract
            INNER JOIN "masterdata"."MasterDataValues" contract_status
                ON contract_status."Id" = contract."StatusId"
               AND contract_status."IsDeleted" = FALSE
               AND contract_status."IsActive" = TRUE
               AND contract_status."Code" = 'ACTIVE'
            INNER JOIN "billing"."Invoices" invoice
                ON invoice."ContractId" = contract."Id"
               AND invoice."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE contract."UnitId" = unit."Id"
              AND contract."IsDeleted" = FALSE
              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) latest_invoice ON TRUE
        WHERE property."PublicId" = ANY(@PropertyPublicIds)
          AND property."IsDeleted" = FALSE
          AND unit_type."Code" <> 'FLOOR'
          AND (@FloorNumber IS NULL OR unit."FloorNumber" = @FloorNumber)
          AND (@RoomStatusCode IS NULL OR unit_status."Code" = @RoomStatusCode)
          AND (@PaymentStatusCode IS NULL OR latest_invoice."PaymentStatusCode" = @PaymentStatusCode)
          AND (
                @Search IS NULL
                OR property."Name" ILIKE '%' || @Search || '%'
                OR property."PropertyCode" ILIKE @Search || '%'
                OR property."FormattedAddress" ILIKE '%' || @Search || '%'
                OR unit."UnitName" ILIKE '%' || @Search || '%'
                OR unit."UnitCode" ILIKE @Search || '%'
          )
        GROUP BY
            property."PublicId",
            unit."FloorNumber"
        ORDER BY property."PublicId", unit."FloorNumber" NULLS LAST;
        """;

    /// <summary>
    /// Loads room rows for selected properties.
    /// </summary>
    public const string GET_PROPERTY_ROOMS_BY_PUBLIC_IDS_QUERY = """
        SELECT
            property."PublicId" AS "PropertyPublicId",
            unit."PublicId" AS "UnitPublicId",
            unit."UnitCode",
            unit."UnitName",
            unit."FloorNumber",
            unit."AreaSqm",
            unit."BaseRentAmount",
            unit."DefaultDepositAmount",
            COALESCE(rent_summary."TotalRentAmount", unit."BaseRentAmount") AS "TotalRentAmount",
            unit."BedCount",
            unit."IsPetAllowed",
            unit_type."Code" AS "UnitTypeCode",
            unit_type."Name" AS "UnitTypeName",
            rental_mode."Code" AS "RentalModeCode",
            rental_mode."Name" AS "RentalModeName",
            unit_status."Code" AS "StatusCode",
            unit_status."Name" AS "StatusName",
            latest_invoice."PaymentStatusCode",
            latest_invoice."PaymentStatusName",
            COALESCE(occupancy_count."OccupiedBedCount", 0)::int AS "OccupiedBedCount",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."RentalChargePolicies" override_policy
                WHERE override_policy."UnitId" = unit."Id"
                  AND override_policy."IsDeleted" = FALSE
            ) AS "UsesCommonChargePolicies",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."UnitPackages" override_package
                INNER JOIN "masterdata"."MasterDataValues" override_package_type
                    ON override_package_type."Id" = override_package."PackageTypeId"
                   AND override_package_type."IsDeleted" = FALSE
                   AND override_package_type."IsActive" = TRUE
                   AND override_package_type."Code" <> @NoFurniturePackageTypeCode
                WHERE override_package."UnitId" = unit."Id"
                  AND override_package."IsDeleted" = FALSE
            ) AS "UsesCommonPackages"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."Units" unit
            ON unit."PropertyId" = property."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" rental_mode
            ON rental_mode."Id" = unit."RentalModeId"
           AND rental_mode."IsDeleted" = FALSE
           AND rental_mode."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" unit_status
            ON unit_status."Id" = unit."StatusId"
           AND unit_status."IsDeleted" = FALSE
           AND unit_status."IsActive" = TRUE
        LEFT JOIN LATERAL (
            SELECT
                COALESCE(contract."RentAmount", unit."BaseRentAmount") AS "TotalRentAmount"
            FROM "leasing"."Occupancies" occupancy
            INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                ON occupancy_status."Id" = occupancy."StatusId"
               AND occupancy_status."IsDeleted" = FALSE
               AND occupancy_status."IsActive" = TRUE
               AND occupancy_status."Code" = 'ACTIVE'
            LEFT JOIN "leasing"."Contracts" contract
                ON contract."Id" = occupancy."ContractId"
               AND contract."IsDeleted" = FALSE
            WHERE occupancy."UnitId" = unit."Id"
              AND occupancy."IsDeleted" = FALSE
              AND occupancy."IsPrimaryTenant" = TRUE
            ORDER BY occupancy."StartDate" DESC NULLS LAST, occupancy."CreatedAt" DESC
            LIMIT 1
        ) rent_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT
                invoice_status."Code" AS "PaymentStatusCode",
                invoice_status."Name" AS "PaymentStatusName"
            FROM "leasing"."Contracts" contract
            INNER JOIN "masterdata"."MasterDataValues" contract_status
                ON contract_status."Id" = contract."StatusId"
               AND contract_status."IsDeleted" = FALSE
               AND contract_status."IsActive" = TRUE
               AND contract_status."Code" = 'ACTIVE'
            INNER JOIN "billing"."Invoices" invoice
                ON invoice."ContractId" = contract."Id"
               AND invoice."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE contract."UnitId" = unit."Id"
              AND contract."IsDeleted" = FALSE
              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) latest_invoice ON TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(occupancy."Id")::int AS "OccupiedBedCount"
            FROM "leasing"."Occupancies" occupancy
            INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                ON occupancy_status."Id" = occupancy."StatusId"
               AND occupancy_status."IsDeleted" = FALSE
               AND occupancy_status."IsActive" = TRUE
               AND occupancy_status."Code" = 'ACTIVE'
            WHERE occupancy."UnitId" = unit."Id"
              AND occupancy."IsDeleted" = FALSE
              AND occupancy."IsPrimaryTenant" = TRUE
        ) occupancy_count ON TRUE
        WHERE property."PublicId" = ANY(@PropertyPublicIds)
          AND property."IsDeleted" = FALSE
          AND unit_type."Code" <> 'FLOOR'
          AND (@FloorNumber IS NULL OR unit."FloorNumber" = @FloorNumber)
          AND (@RoomStatusCode IS NULL OR unit_status."Code" = @RoomStatusCode)
          AND (@PaymentStatusCode IS NULL OR latest_invoice."PaymentStatusCode" = @PaymentStatusCode)
          AND (
                @Search IS NULL
                OR property."Name" ILIKE '%' || @Search || '%'
                OR property."PropertyCode" ILIKE @Search || '%'
                OR property."FormattedAddress" ILIKE '%' || @Search || '%'
                OR unit."UnitName" ILIKE '%' || @Search || '%'
                OR unit."UnitCode" ILIKE @Search || '%'
          )
        ORDER BY property."PublicId", unit."FloorNumber" NULLS LAST, unit."UnitCode";
        """;

    /// <summary>
    /// Loads active tenants for selected property room rows.
    /// </summary>
    public const string GET_PROPERTY_ROOM_TENANTS_BY_PUBLIC_IDS_QUERY = """
        SELECT
            property."PublicId" AS "PropertyPublicId",
            unit."PublicId" AS "UnitPublicId",
            occupancy."PublicId" AS "OccupancyPublicId",
            tenant_party."PublicId" AS "TenantPublicId",
            tenant_party."DisplayName" AS "TenantName",
            CASE WHEN occupancy."IsPrimaryTenant" THEN 'PRIMARY' ELSE 'OCCUPANT' END AS "RoleCode",
            COALESCE(contract."EffectiveFrom", occupancy."StartDate")::timestamp AS "ContractStartDate",
            COALESCE(contract."EffectiveTo", occupancy."EndDate")::timestamp AS "ContractEndDate",
            occupancy_status."Code" AS "OccupancyStatusCode",
            occupancy_status."Name" AS "OccupancyStatusName"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."Units" unit
            ON unit."PropertyId" = property."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" unit_status
            ON unit_status."Id" = unit."StatusId"
           AND unit_status."IsDeleted" = FALSE
           AND unit_status."IsActive" = TRUE
        INNER JOIN "leasing"."Occupancies" occupancy
            ON occupancy."UnitId" = unit."Id"
           AND occupancy."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" occupancy_status
            ON occupancy_status."Id" = occupancy."StatusId"
           AND occupancy_status."IsDeleted" = FALSE
           AND occupancy_status."IsActive" = TRUE
           AND occupancy_status."Code" = 'ACTIVE'
        INNER JOIN "core"."Parties" tenant_party
            ON tenant_party."Id" = occupancy."PartyId"
           AND tenant_party."IsDeleted" = FALSE
        LEFT JOIN "leasing"."Contracts" contract
            ON contract."Id" = occupancy."ContractId"
           AND contract."IsDeleted" = FALSE
        LEFT JOIN LATERAL (
            SELECT invoice_status."Code" AS "PaymentStatusCode"
            FROM "billing"."Invoices" invoice
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE invoice."ContractId" = contract."Id"
              AND invoice."IsDeleted" = FALSE
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) latest_invoice ON TRUE
        WHERE property."PublicId" = ANY(@PropertyPublicIds)
          AND property."IsDeleted" = FALSE
          AND unit_type."Code" <> 'FLOOR'
          AND (@FloorNumber IS NULL OR unit."FloorNumber" = @FloorNumber)
          AND (@RoomStatusCode IS NULL OR unit_status."Code" = @RoomStatusCode)
          AND (@PaymentStatusCode IS NULL OR latest_invoice."PaymentStatusCode" = @PaymentStatusCode)
          AND (
                @Search IS NULL
                OR property."Name" ILIKE '%' || @Search || '%'
                OR property."PropertyCode" ILIKE @Search || '%'
                OR property."FormattedAddress" ILIKE '%' || @Search || '%'
                OR unit."UnitName" ILIKE '%' || @Search || '%'
                OR unit."UnitCode" ILIKE @Search || '%'
          )
        ORDER BY property."PublicId", unit."FloorNumber" NULLS LAST, unit."UnitCode", tenant_party."DisplayName";
        """;

    /// <summary>
    /// Loads distinct package template source rows for a property detail.
    /// </summary>
    public const string GET_PROPERTY_PACKAGE_TEMPLATES_QUERY = """
        SELECT
            unit_package."PublicId" AS "PackagePublicId",
            unit_package."PackageName",
            unit_package."PriceAdjustment",
            package_item."ItemName",
            package_item."DisplayOrder"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."UnitPackages" unit_package
            ON unit_package."PropertyId" = property."Id"
           AND unit_package."UnitId" IS NULL
           AND unit_package."IsDeleted" = FALSE
        LEFT JOIN "asset"."UnitPackageItems" package_item
            ON package_item."UnitPackageId" = unit_package."Id"
           AND package_item."IsDeleted" = FALSE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE
        ORDER BY unit_package."PackageName", package_item."DisplayOrder" NULLS LAST, package_item."ItemName";
        """;

    /// <summary>
    /// Loads property-level rental charge policies.
    /// </summary>
    public const string GET_PROPERTY_CHARGE_POLICIES_QUERY = """
        SELECT
            policy."PublicId" AS "PolicyPublicId",
            line_type."Code" AS "ChargeTypeCode",
            line_type."Name" AS "ChargeTypeName",
            vehicle_type."Code" AS "VehicleTypeCode",
            vehicle_type."Name" AS "VehicleTypeName",
            policy."ChargeName",
            policy."IsUsageBased",
            policy."Amount",
            policy_status."Code" AS "StatusCode",
            policy_status."Name" AS "StatusName"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."RentalChargePolicies" policy
            ON policy."PropertyId" = property."Id"
           AND policy."UnitId" IS NULL
           AND policy."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" line_type
            ON line_type."Id" = policy."LineTypeId"
           AND line_type."IsDeleted" = FALSE
           AND line_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" policy_status
            ON policy_status."Id" = policy."StatusId"
           AND policy_status."IsDeleted" = FALSE
           AND policy_status."IsActive" = TRUE
        LEFT JOIN "masterdata"."MasterDataValues" vehicle_type
            ON vehicle_type."Id" = policy."VehicleTypeId"
           AND vehicle_type."IsDeleted" = FALSE
           AND vehicle_type."IsActive" = TRUE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE
        ORDER BY policy."CreatedAt", policy."Id";
        """;

    /// <summary>
    /// Loads the whole-building representative unit and its active contract for property detail.
    /// </summary>
    public const string GET_PROPERTY_WHOLE_BUILDING_RENTAL_QUERY = """
        SELECT
            whole_building_contract."ContractPublicId",
            whole_building_contract."ContractCode",
            whole_building_contract."TenantPublicId",
            whole_building_contract."TenantName",
            whole_building_contract."StatusCode",
            whole_building_contract."StatusName",
            whole_building_contract."TotalRentAmount",
            whole_building_contract."StartDate",
            whole_building_contract."EndDate"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."Units" unit
            ON unit."PropertyId" = property."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
           AND unit_type."Code" = 'BUILDING'
        LEFT JOIN LATERAL (
            SELECT
                contract."PublicId" AS "ContractPublicId",
                contract."ContractCode",
                tenant_party."PublicId" AS "TenantPublicId",
                tenant_party."DisplayName" AS "TenantName",
                contract_status."Code" AS "StatusCode",
                contract_status."Name" AS "StatusName",
                contract."RentAmount" AS "TotalRentAmount",
                contract."EffectiveFrom"::timestamp AS "StartDate",
                contract."EffectiveTo"::timestamp AS "EndDate"
            FROM "leasing"."Contracts" contract
            INNER JOIN "masterdata"."MasterDataValues" contract_status
                ON contract_status."Id" = contract."StatusId"
               AND contract_status."IsDeleted" = FALSE
               AND contract_status."IsActive" = TRUE
            LEFT JOIN "core"."Parties" tenant_party
                ON tenant_party."Id" = contract."SecondaryPartyId"
               AND tenant_party."IsDeleted" = FALSE
            WHERE contract."UnitId" = unit."Id"
              AND contract."IsDeleted" = FALSE
              AND contract_status."Code" IN ('ACTIVE', 'SCHEDULED', 'SIGNED')
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
            ORDER BY
                CASE contract_status."Code"
                    WHEN 'ACTIVE' THEN 0
                    WHEN 'SCHEDULED' THEN 1
                    WHEN 'SIGNED' THEN 2
                    ELSE 3
                END,
                contract."EffectiveFrom" DESC,
                contract."CreatedAt" DESC
            LIMIT 1
        ) whole_building_contract ON TRUE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE
          AND (
                whole_building_contract."ContractPublicId" IS NOT NULL
                OR NOT EXISTS (
                    SELECT 1
                    FROM "asset"."Units" rented_unit
                    INNER JOIN "masterdata"."MasterDataValues" rented_unit_type
                        ON rented_unit_type."Id" = rented_unit."UnitTypeId"
                       AND rented_unit_type."IsDeleted" = FALSE
                       AND rented_unit_type."IsActive" = TRUE
                       AND rented_unit_type."Code" <> 'BUILDING'
                    LEFT JOIN "leasing"."Contracts" rented_contract
                        ON rented_contract."UnitId" = rented_unit."Id"
                       AND rented_contract."IsDeleted" = FALSE
                    LEFT JOIN "masterdata"."MasterDataValues" rented_contract_status
                        ON rented_contract_status."Id" = rented_contract."StatusId"
                       AND rented_contract_status."IsDeleted" = FALSE
                       AND rented_contract_status."IsActive" = TRUE
                       AND rented_contract_status."Code" IN ('ACTIVE', 'SCHEDULED', 'SIGNED')
                    LEFT JOIN "leasing"."Occupancies" rented_occupancy
                        ON rented_occupancy."UnitId" = rented_unit."Id"
                       AND rented_occupancy."IsDeleted" = FALSE
                    LEFT JOIN "masterdata"."MasterDataValues" rented_occupancy_status
                        ON rented_occupancy_status."Id" = rented_occupancy."StatusId"
                       AND rented_occupancy_status."IsDeleted" = FALSE
                       AND rented_occupancy_status."IsActive" = TRUE
                       AND rented_occupancy_status."Code" = 'ACTIVE'
                    WHERE rented_unit."PropertyId" = property."Id"
                      AND rented_unit."IsDeleted" = FALSE
                      AND (
                            rented_contract_status."Id" IS NOT NULL
                            OR rented_occupancy_status."Id" IS NOT NULL
                          )
                )
          )
        ORDER BY
            (whole_building_contract."ContractPublicId" IS NULL),
            unit."CreatedAt" DESC
        LIMIT 1;
        """;

    /// <summary>
    /// Loads one page of room ledger rows scoped to the current party.
    /// </summary>
    public const string GET_ROOM_LIST_PAGE_QUERY = """
        WITH scoped_units AS (
            SELECT
                unit."Id",
                CASE
                    WHEN @Search IS NULL THEN 0
                    WHEN unit."UnitCode" ILIKE @Search || '%' THEN 0
                    WHEN unit."UnitName" ILIKE '%' || @Search || '%' THEN 1
                    ELSE 2
                END AS "SearchRank"
            FROM "asset"."Units" unit
            INNER JOIN "asset"."Properties" property
                ON property."Id" = unit."PropertyId"
               AND property."IsDeleted" = FALSE
            INNER JOIN "asset"."PropertyParties" property_party
                ON property_party."PropertyId" = property."Id"
               AND property_party."IsDeleted" = FALSE
               AND property_party."PartyId" = @CurrentPartyId
               AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
               AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
            INNER JOIN "masterdata"."MasterDataValues" relationship_type
                ON relationship_type."Id" = property_party."RelationshipTypeId"
               AND relationship_type."IsDeleted" = FALSE
               AND relationship_type."IsActive" = TRUE
               AND relationship_type."Code" = ANY(@RelationshipCodes)
            INNER JOIN "masterdata"."MasterDataValues" unit_type
                ON unit_type."Id" = unit."UnitTypeId"
               AND unit_type."IsDeleted" = FALSE
               AND unit_type."IsActive" = TRUE
               AND unit_type."Code" <> 'FLOOR'
            INNER JOIN "masterdata"."MasterDataValues" unit_status
                ON unit_status."Id" = unit."StatusId"
               AND unit_status."IsDeleted" = FALSE
               AND unit_status."IsActive" = TRUE
            INNER JOIN "masterdata"."MasterDataValues" rental_mode
                ON rental_mode."Id" = unit."RentalModeId"
               AND rental_mode."IsDeleted" = FALSE
               AND rental_mode."IsActive" = TRUE
            LEFT JOIN LATERAL (
                SELECT invoice_status."Code" AS "PaymentStatusCode"
                FROM "leasing"."Contracts" contract
                INNER JOIN "masterdata"."MasterDataValues" contract_status
                    ON contract_status."Id" = contract."StatusId"
                   AND contract_status."IsDeleted" = FALSE
                   AND contract_status."IsActive" = TRUE
                   AND contract_status."Code" = 'ACTIVE'
                INNER JOIN "billing"."Invoices" invoice
                    ON invoice."ContractId" = contract."Id"
                   AND invoice."IsDeleted" = FALSE
                INNER JOIN "masterdata"."MasterDataValues" invoice_status
                    ON invoice_status."Id" = invoice."StatusId"
                   AND invoice_status."IsDeleted" = FALSE
                   AND invoice_status."IsActive" = TRUE
                WHERE contract."UnitId" = unit."Id"
                  AND contract."IsDeleted" = FALSE
                  AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
                  AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
                ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
                LIMIT 1
            ) latest_invoice ON TRUE
            WHERE unit."IsDeleted" = FALSE
              AND (@PropertyId IS NULL OR property."PublicId" = @PropertyId)
              AND (
                    @PropertySearch IS NULL
                    OR property."PropertyCode" ILIKE @PropertySearch || '%'
                    OR property."Name" ILIKE '%' || @PropertySearch || '%'
                    OR property."FormattedAddress" ILIKE '%' || @PropertySearch || '%'
              )
              AND (
                    @Search IS NULL
                    OR unit."UnitCode" ILIKE @Search || '%'
                    OR unit."UnitName" ILIKE '%' || @Search || '%'
              )
              AND (@FloorNumber IS NULL OR unit."FloorNumber" = @FloorNumber)
              AND (@RoomStatusCode IS NULL OR unit_status."Code" = @RoomStatusCode)
              AND (@RentalModeCode IS NULL OR rental_mode."Code" = @RentalModeCode)
              AND (@PaymentStatusCode IS NULL OR latest_invoice."PaymentStatusCode" = @PaymentStatusCode)
        ),
        paged_units AS (
            SELECT
                scoped."Id",
                scoped."SearchRank",
                COUNT(*) OVER()::int AS "TotalCount"
            FROM scoped_units scoped
            INNER JOIN "asset"."Units" unit
                ON unit."Id" = scoped."Id"
               AND unit."IsDeleted" = FALSE
            ORDER BY scoped."SearchRank", unit."FloorNumber" NULLS LAST, unit."UnitCode", unit."CreatedAt"
            LIMIT @PageSize OFFSET @Offset
        )
        SELECT
            paged."TotalCount",
            property."PublicId" AS "PropertyPublicId",
            property."PropertyCode",
            property."Name" AS "PropertyName",
            COALESCE(NULLIF(property."FormattedAddress", ''), property."StreetAddress") AS "PropertyAddress",
            COALESCE(property_summary."TotalRooms", 0)::int AS "PropertyTotalRooms",
            COALESCE(property_summary."AvailableRooms", 0)::int AS "PropertyAvailableRooms",
            unit."FloorNumber",
            COALESCE(floor_summary."TotalRooms", 0)::int AS "FloorTotalRooms",
            COALESCE(floor_summary."AvailableRooms", 0)::int AS "FloorAvailableRooms",
            unit."PublicId" AS "UnitPublicId",
            unit."UnitCode",
            unit."UnitName",
            unit_type."Code" AS "UnitTypeCode",
            unit_type."Name" AS "UnitTypeName",
            rental_mode."Code" AS "RentalModeCode",
            rental_mode."Name" AS "RentalModeName",
            unit_status."Code" AS "StatusCode",
            unit_status."Name" AS "StatusName",
            COALESCE(active_contract."RentAmount", unit."BaseRentAmount", 0) AS "TotalRentAmount",
            COALESCE(occupied_beds."TotalOccupiedBeds", 0)::int AS "TotalOccupiedBeds",
            unit."BedCount" AS "TotalBeds",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."RentalChargePolicies" room_policy
                WHERE room_policy."UnitId" = unit."Id"
                  AND room_policy."IsDeleted" = FALSE
            ) AS "UsesCommonChargePolicies",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."UnitPackages" room_package
                INNER JOIN "masterdata"."MasterDataValues" room_package_type
                    ON room_package_type."Id" = room_package."PackageTypeId"
                   AND room_package_type."IsDeleted" = FALSE
                   AND room_package_type."IsActive" = TRUE
                   AND room_package_type."Code" <> @NoFurniturePackageTypeCode
                WHERE room_package."UnitId" = unit."Id"
                  AND room_package."IsDeleted" = FALSE
            ) AS "UsesCommonPackages"
        FROM paged_units paged
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = paged."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" rental_mode
            ON rental_mode."Id" = unit."RentalModeId"
           AND rental_mode."IsDeleted" = FALSE
           AND rental_mode."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" unit_status
            ON unit_status."Id" = unit."StatusId"
           AND unit_status."IsDeleted" = FALSE
           AND unit_status."IsActive" = TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(property_unit."Id")::int AS "TotalRooms",
                   COUNT(property_unit."Id") FILTER (WHERE property_unit_status."Code" = 'AVAILABLE')::int AS "AvailableRooms"
            FROM "asset"."Units" property_unit
            INNER JOIN "masterdata"."MasterDataValues" property_unit_type
                ON property_unit_type."Id" = property_unit."UnitTypeId"
               AND property_unit_type."IsDeleted" = FALSE
               AND property_unit_type."IsActive" = TRUE
               AND property_unit_type."Code" <> 'FLOOR'
            INNER JOIN "masterdata"."MasterDataValues" property_unit_status
                ON property_unit_status."Id" = property_unit."StatusId"
               AND property_unit_status."IsDeleted" = FALSE
               AND property_unit_status."IsActive" = TRUE
            WHERE property_unit."PropertyId" = property."Id"
              AND property_unit."IsDeleted" = FALSE
        ) property_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(floor_unit."Id")::int AS "TotalRooms",
                   COUNT(floor_unit."Id") FILTER (WHERE floor_unit_status."Code" = 'AVAILABLE')::int AS "AvailableRooms"
            FROM "asset"."Units" floor_unit
            INNER JOIN "masterdata"."MasterDataValues" floor_unit_type
                ON floor_unit_type."Id" = floor_unit."UnitTypeId"
               AND floor_unit_type."IsDeleted" = FALSE
               AND floor_unit_type."IsActive" = TRUE
               AND floor_unit_type."Code" <> 'FLOOR'
            INNER JOIN "masterdata"."MasterDataValues" floor_unit_status
                ON floor_unit_status."Id" = floor_unit."StatusId"
               AND floor_unit_status."IsDeleted" = FALSE
               AND floor_unit_status."IsActive" = TRUE
            WHERE floor_unit."PropertyId" = property."Id"
              AND floor_unit."FloorNumber" IS NOT DISTINCT FROM unit."FloorNumber"
              AND floor_unit."IsDeleted" = FALSE
        ) floor_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT contract."RentAmount"
            FROM "leasing"."Contracts" contract
            INNER JOIN "masterdata"."MasterDataValues" contract_status
                ON contract_status."Id" = contract."StatusId"
               AND contract_status."IsDeleted" = FALSE
               AND contract_status."IsActive" = TRUE
               AND contract_status."Code" = 'ACTIVE'
            WHERE contract."UnitId" = unit."Id"
              AND contract."IsDeleted" = FALSE
              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
            ORDER BY contract."EffectiveFrom" DESC NULLS LAST, contract."CreatedAt" DESC
            LIMIT 1
        ) active_contract ON TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(occupancy."Id")::int AS "TotalOccupiedBeds"
            FROM "leasing"."Occupancies" occupancy
            INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                ON occupancy_status."Id" = occupancy."StatusId"
               AND occupancy_status."IsDeleted" = FALSE
               AND occupancy_status."IsActive" = TRUE
               AND occupancy_status."Code" = 'ACTIVE'
            WHERE occupancy."UnitId" = unit."Id"
              AND occupancy."IsDeleted" = FALSE
              AND occupancy."IsPrimaryTenant" = TRUE
        ) occupied_beds ON TRUE
        ORDER BY paged."SearchRank", property."Name", unit."FloorNumber" NULLS LAST, unit."UnitCode";
        """;

    /// <summary>
    /// Loads active tenant rows for selected rooms.
    /// </summary>
    public const string GET_ROOM_TENANTS_BY_PUBLIC_IDS_QUERY = """
        SELECT
            unit."PublicId" AS "UnitPublicId",
            occupancy."PublicId" AS "OccupancyPublicId",
            tenant_party."PublicId" AS "TenantPublicId",
            tenant_party."DisplayName" AS "TenantName",
            CASE WHEN occupancy."IsPrimaryTenant" THEN 'PRIMARY' ELSE 'OCCUPANT' END AS "RoleCode",
            COALESCE(contract."EffectiveFrom", occupancy."StartDate")::timestamp AS "ContractStartDate",
            COALESCE(contract."EffectiveTo", occupancy."EndDate")::timestamp AS "ContractEndDate",
            occupancy_status."Code" AS "OccupancyStatusCode",
            occupancy_status."Name" AS "OccupancyStatusName",
            deposit_status."Code" AS "DepositStatusCode",
            deposit_status."Name" AS "DepositStatusName"
        FROM "asset"."Units" unit
        INNER JOIN "leasing"."Occupancies" occupancy
            ON occupancy."UnitId" = unit."Id"
           AND occupancy."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" occupancy_status
            ON occupancy_status."Id" = occupancy."StatusId"
           AND occupancy_status."IsDeleted" = FALSE
           AND occupancy_status."IsActive" = TRUE
           AND occupancy_status."Code" = 'ACTIVE'
        INNER JOIN "core"."Parties" tenant_party
            ON tenant_party."Id" = occupancy."PartyId"
           AND tenant_party."IsDeleted" = FALSE
        LEFT JOIN "leasing"."Contracts" contract
            ON contract."Id" = occupancy."ContractId"
           AND contract."IsDeleted" = FALSE
        LEFT JOIN LATERAL (
            SELECT invoice_status."Code", invoice_status."Name"
            FROM "billing"."Invoices" invoice
            INNER JOIN "masterdata"."MasterDataValues" invoice_type
                ON invoice_type."Id" = invoice."InvoiceTypeId"
               AND invoice_type."IsDeleted" = FALSE
               AND invoice_type."IsActive" = TRUE
               AND invoice_type."Code" = 'DEPOSIT'
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE invoice."ContractId" = contract."Id"
              AND invoice."IsDeleted" = FALSE
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) deposit_status ON TRUE
        WHERE unit."PublicId" = ANY(@UnitPublicIds)
          AND unit."IsDeleted" = FALSE
        ORDER BY unit."PublicId", tenant_party."DisplayName";
        """;

    /// <summary>
    /// Loads one page of landlord-scoped tenant occupancies.
    /// </summary>
    public const string GET_TENANT_LIST_PAGE_QUERY = """
        SELECT
            occupancy."PublicId" AS "OccupancyPublicId",
            tenant_party."PublicId" AS "TenantPublicId",
            tenant_party."DisplayName" AS "TenantName",
            tenant_party."PrimaryPhone" AS "Phone",
            tenant_party."PrimaryEmail" AS "Email",
            CASE WHEN occupancy."IsPrimaryTenant" THEN 'PRIMARY' ELSE 'OCCUPANT' END AS "RoleCode",
            property."PublicId" AS "PropertyPublicId",
            property."Name" AS "PropertyName",
            unit."PublicId" AS "RoomPublicId",
            unit."UnitCode" AS "RoomCode",
            unit."UnitName" AS "RoomName",
            COALESCE(contract."EffectiveFrom", occupancy."StartDate")::timestamp AS "ContractStartDate",
            COALESCE(contract."EffectiveTo", occupancy."EndDate")::timestamp AS "ContractEndDate",
            contract."RentAmount" AS "ContractRentAmount",
            deposit_status."Code" AS "DepositStatusCode",
            deposit_status."Name" AS "DepositStatusName",
            occupancy_status."Code" AS "OccupancyStatusCode",
            occupancy_status."Name" AS "OccupancyStatusName",
            COUNT(*) OVER() AS "TotalCount"
        FROM "leasing"."Occupancies" occupancy
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = occupancy."UnitId"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property
            ON property."Id" = occupancy."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."PartyId" = @CurrentPartyId
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        INNER JOIN "core"."Parties" tenant_party
            ON tenant_party."Id" = occupancy."PartyId"
           AND tenant_party."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" occupancy_status
            ON occupancy_status."Id" = occupancy."StatusId"
           AND occupancy_status."IsDeleted" = FALSE
           AND occupancy_status."IsActive" = TRUE
        LEFT JOIN "leasing"."Contracts" contract
            ON contract."Id" = occupancy."ContractId"
           AND contract."IsDeleted" = FALSE
        LEFT JOIN LATERAL (
            SELECT invoice_status."Code", invoice_status."Name"
            FROM "billing"."Invoices" invoice
            INNER JOIN "masterdata"."MasterDataValues" invoice_type
                ON invoice_type."Id" = invoice."InvoiceTypeId"
               AND invoice_type."IsDeleted" = FALSE
               AND invoice_type."IsActive" = TRUE
               AND invoice_type."Code" = 'DEPOSIT'
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE invoice."ContractId" = contract."Id"
              AND invoice."IsDeleted" = FALSE
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) deposit_status ON TRUE
        WHERE occupancy."IsDeleted" = FALSE
          AND (@PropertyId IS NULL OR property."PublicId" = @PropertyId)
          AND (@RoomId IS NULL OR unit."PublicId" = @RoomId)
          AND (@OccupancyStatusCode IS NULL OR occupancy_status."Code" = @OccupancyStatusCode)
          AND (
                @RoleCode IS NULL
                OR (@RoleCode = 'PRIMARY' AND occupancy."IsPrimaryTenant" = TRUE)
                OR (@RoleCode = 'OCCUPANT' AND occupancy."IsPrimaryTenant" = FALSE)
          )
          AND (
                @Search IS NULL
                OR tenant_party."DisplayName" ILIKE '%' || @Search || '%'
                OR tenant_party."PrimaryPhone" ILIKE @Search || '%'
                OR tenant_party."PrimaryEmail" ILIKE '%' || @Search || '%'
                OR property."Name" ILIKE '%' || @Search || '%'
                OR unit."UnitName" ILIKE '%' || @Search || '%'
                OR unit."UnitCode" ILIKE @Search || '%'
          )
        ORDER BY tenant_party."DisplayName", property."Name", unit."FloorNumber" NULLS LAST, unit."UnitCode"
        LIMIT @PageSize OFFSET @Offset;
        """;

    /// <summary>
    /// Loads one landlord-scoped tenant occupancy detail.
    /// </summary>
    public const string GET_TENANT_DETAIL_QUERY = """
        SELECT
            occupancy."PublicId" AS "OccupancyPublicId",
            tenant_party."PublicId" AS "TenantPublicId",
            tenant_party."DisplayName" AS "TenantName",
            tenant_party."PrimaryPhone" AS "Phone",
            tenant_party."PrimaryEmail" AS "Email",
            CASE WHEN occupancy."IsPrimaryTenant" THEN 'PRIMARY' ELSE 'OCCUPANT' END AS "RoleCode",
            property."PublicId" AS "PropertyPublicId",
            property."Name" AS "PropertyName",
            unit."PublicId" AS "RoomPublicId",
            unit."UnitCode" AS "RoomCode",
            unit."UnitName" AS "RoomName",
            COALESCE(contract."EffectiveFrom", occupancy."StartDate")::timestamp AS "ContractStartDate",
            COALESCE(contract."EffectiveTo", occupancy."EndDate")::timestamp AS "ContractEndDate",
            contract."RentAmount" AS "ContractRentAmount",
            deposit_status."Code" AS "DepositStatusCode",
            deposit_status."Name" AS "DepositStatusName",
            occupancy_status."Code" AS "OccupancyStatusCode",
            occupancy_status."Name" AS "OccupancyStatusName"
        FROM "leasing"."Occupancies" occupancy
        INNER JOIN "asset"."Units" unit
            ON unit."Id" = occupancy."UnitId"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property
            ON property."Id" = occupancy."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."PartyId" = @CurrentPartyId
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        INNER JOIN "core"."Parties" tenant_party
            ON tenant_party."Id" = occupancy."PartyId"
           AND tenant_party."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" occupancy_status
            ON occupancy_status."Id" = occupancy."StatusId"
           AND occupancy_status."IsDeleted" = FALSE
           AND occupancy_status."IsActive" = TRUE
        LEFT JOIN "leasing"."Contracts" contract
            ON contract."Id" = occupancy."ContractId"
           AND contract."IsDeleted" = FALSE
        LEFT JOIN LATERAL (
            SELECT invoice_status."Code", invoice_status."Name"
            FROM "billing"."Invoices" invoice
            INNER JOIN "masterdata"."MasterDataValues" invoice_type
                ON invoice_type."Id" = invoice."InvoiceTypeId"
               AND invoice_type."IsDeleted" = FALSE
               AND invoice_type."IsActive" = TRUE
               AND invoice_type."Code" = 'DEPOSIT'
            INNER JOIN "masterdata"."MasterDataValues" invoice_status
                ON invoice_status."Id" = invoice."StatusId"
               AND invoice_status."IsDeleted" = FALSE
               AND invoice_status."IsActive" = TRUE
            WHERE invoice."ContractId" = contract."Id"
              AND invoice."IsDeleted" = FALSE
            ORDER BY invoice."DueDate" DESC NULLS LAST, invoice."CreatedAt" DESC
            LIMIT 1
        ) deposit_status ON TRUE
        WHERE occupancy."PublicId" = @OccupancyPublicId
          AND occupancy."IsDeleted" = FALSE;
        """;

    /// <summary>
    /// Loads compact room context for a tenant QR join token.
    /// </summary>
    public const string GET_TENANT_JOIN_ROOM_QUERY = """
        SELECT
            property."Id" AS "PropertyId",
            property."PublicId" AS "PropertyPublicId",
            property."Name" AS "PropertyName",
            unit."Id" AS "UnitId",
            unit."PublicId" AS "RoomPublicId",
            unit."UnitCode" AS "RoomCode",
            unit."UnitName" AS "RoomName",
            unit."RentalModeId",
            unit."BedCount"
        FROM "asset"."Units" unit
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."IsDeleted" = FALSE
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "core"."Parties" landlord_party
            ON landlord_party."Id" = property_party."PartyId"
           AND landlord_party."PublicId" = @LandlordPartyPublicId
           AND landlord_party."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        WHERE unit."PublicId" = @RoomPublicId
          AND unit."IsDeleted" = FALSE
        LIMIT 1;
        """;

    /// <summary>
    /// Loads one room detail header scoped to the current party.
    /// </summary>
    public const string GET_ROOM_DETAIL_QUERY = """
        SELECT
            unit."Id" AS "UnitId",
            unit."PublicId" AS "UnitPublicId",
            unit."UnitCode",
            unit."UnitName",
            unit."FloorNumber",
            property."PublicId" AS "PropertyPublicId",
            property."PropertyCode",
            property."Name" AS "PropertyName",
            COALESCE(NULLIF(property."FormattedAddress", ''), property."StreetAddress") AS "PropertyAddress",
            unit_type."Code" AS "UnitTypeCode",
            unit_type."Name" AS "UnitTypeName",
            rental_mode."Code" AS "RentalModeCode",
            rental_mode."Name" AS "RentalModeName",
            unit_status."Code" AS "StatusCode",
            unit_status."Name" AS "StatusName",
            unit."AreaSqm",
            unit."BaseRentAmount",
            unit."DefaultDepositAmount",
            unit."BedCount" AS "TotalBeds",
            unit."IsPetAllowed",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."RentalChargePolicies" room_policy
                WHERE room_policy."UnitId" = unit."Id"
                  AND room_policy."IsDeleted" = FALSE
            ) AS "UsesCommonChargePolicies",
            NOT EXISTS (
                SELECT 1
                FROM "asset"."UnitPackages" room_package
                INNER JOIN "masterdata"."MasterDataValues" room_package_type
                    ON room_package_type."Id" = room_package."PackageTypeId"
                   AND room_package_type."IsDeleted" = FALSE
                   AND room_package_type."IsActive" = TRUE
                   AND room_package_type."Code" <> @NoFurniturePackageTypeCode
                WHERE room_package."UnitId" = unit."Id"
                  AND room_package."IsDeleted" = FALSE
            ) AS "UsesCommonPackages"
        FROM "asset"."Units" unit
        INNER JOIN "asset"."Properties" property
            ON property."Id" = unit."PropertyId"
           AND property."IsDeleted" = FALSE
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."IsDeleted" = FALSE
           AND property_party."PartyId" = @CurrentPartyId
           AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
           AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
        INNER JOIN "masterdata"."MasterDataValues" relationship_type
            ON relationship_type."Id" = property_party."RelationshipTypeId"
           AND relationship_type."IsDeleted" = FALSE
           AND relationship_type."IsActive" = TRUE
           AND relationship_type."Code" = ANY(@RelationshipCodes)
        INNER JOIN "masterdata"."MasterDataValues" unit_type
            ON unit_type."Id" = unit."UnitTypeId"
           AND unit_type."IsDeleted" = FALSE
           AND unit_type."IsActive" = TRUE
           AND unit_type."Code" <> 'FLOOR'
        INNER JOIN "masterdata"."MasterDataValues" rental_mode
            ON rental_mode."Id" = unit."RentalModeId"
           AND rental_mode."IsDeleted" = FALSE
           AND rental_mode."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" unit_status
            ON unit_status."Id" = unit."StatusId"
           AND unit_status."IsDeleted" = FALSE
           AND unit_status."IsActive" = TRUE
        WHERE unit."PublicId" = @RoomPublicId
          AND unit."IsDeleted" = FALSE
        LIMIT 1;
        """;

    /// <summary>
    /// Loads effective charge policies for one room.
    /// </summary>
    public const string GET_ROOM_EFFECTIVE_CHARGE_POLICIES_QUERY = """
        WITH scoped_unit AS (
            SELECT unit."Id", unit."PropertyId"
            FROM "asset"."Units" unit
            INNER JOIN "asset"."PropertyParties" property_party
                ON property_party."PropertyId" = unit."PropertyId"
               AND property_party."PartyId" = @CurrentPartyId
               AND property_party."IsDeleted" = FALSE
               AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
               AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
            INNER JOIN "masterdata"."MasterDataValues" relationship_type
                ON relationship_type."Id" = property_party."RelationshipTypeId"
               AND relationship_type."IsDeleted" = FALSE
               AND relationship_type."IsActive" = TRUE
               AND relationship_type."Code" = ANY(@RelationshipCodes)
            WHERE unit."PublicId" = @RoomPublicId
              AND unit."IsDeleted" = FALSE
            LIMIT 1
        ),
        source_scope AS (
            SELECT EXISTS (
                SELECT 1
                FROM "asset"."RentalChargePolicies" room_policy
                INNER JOIN scoped_unit unit
                    ON unit."Id" = room_policy."UnitId"
                WHERE room_policy."IsDeleted" = FALSE
            ) AS "HasRoomOverride"
        )
        SELECT
            policy."PublicId" AS "PolicyPublicId",
            line_type."Code" AS "ChargeTypeCode",
            line_type."Name" AS "ChargeTypeName",
            vehicle_type."Code" AS "VehicleTypeCode",
            vehicle_type."Name" AS "VehicleTypeName",
            policy."ChargeName",
            policy."IsUsageBased",
            policy."Amount",
            policy_status."Code" AS "StatusCode",
            policy_status."Name" AS "StatusName"
        FROM scoped_unit unit
        CROSS JOIN source_scope source
        INNER JOIN "asset"."RentalChargePolicies" policy
            ON policy."PropertyId" = unit."PropertyId"
           AND policy."IsDeleted" = FALSE
           AND (
                (source."HasRoomOverride" = TRUE AND policy."UnitId" = unit."Id")
                OR (source."HasRoomOverride" = FALSE AND policy."UnitId" IS NULL)
           )
        INNER JOIN "masterdata"."MasterDataValues" line_type
            ON line_type."Id" = policy."LineTypeId"
           AND line_type."IsDeleted" = FALSE
           AND line_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" policy_status
            ON policy_status."Id" = policy."StatusId"
           AND policy_status."IsDeleted" = FALSE
           AND policy_status."IsActive" = TRUE
        LEFT JOIN "masterdata"."MasterDataValues" vehicle_type
            ON vehicle_type."Id" = policy."VehicleTypeId"
           AND vehicle_type."IsDeleted" = FALSE
           AND vehicle_type."IsActive" = TRUE
        ORDER BY policy."CreatedAt", policy."Id";
        """;

    /// <summary>
    /// Loads effective package templates for one room.
    /// </summary>
    public const string GET_ROOM_EFFECTIVE_PACKAGE_TEMPLATES_QUERY = """
          WITH scoped_unit AS (
              SELECT unit."Id", unit."PropertyId"
              FROM "asset"."Units" unit
              INNER JOIN "asset"."PropertyParties" property_party
                  ON property_party."PropertyId" = unit."PropertyId"
                 AND property_party."PartyId" = @CurrentPartyId
                 AND property_party."IsDeleted" = FALSE
                 AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                 AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE)
              INNER JOIN "masterdata"."MasterDataValues" relationship_type
                  ON relationship_type."Id" = property_party."RelationshipTypeId"
                 AND relationship_type."IsDeleted" = FALSE
                 AND relationship_type."IsActive" = TRUE
                 AND relationship_type."Code" = ANY(@RelationshipCodes)
              WHERE unit."PublicId" = @RoomPublicId
                AND unit."IsDeleted" = FALSE
            LIMIT 1
        ),
        source_scope AS (
            SELECT EXISTS (
                SELECT 1
                FROM "asset"."UnitPackages" room_package
                INNER JOIN "masterdata"."MasterDataValues" room_package_type
                    ON room_package_type."Id" = room_package."PackageTypeId"
                   AND room_package_type."IsDeleted" = FALSE
                   AND room_package_type."IsActive" = TRUE
                   AND room_package_type."Code" <> @NoFurniturePackageTypeCode
                INNER JOIN scoped_unit unit
                    ON unit."Id" = room_package."UnitId"
                WHERE room_package."IsDeleted" = FALSE
            ) AS "HasRoomOverride"
        )
        SELECT
            unit_package."PublicId" AS "PackagePublicId",
            unit_package."PackageName",
            unit_package."PriceAdjustment",
            package_item."ItemName",
            package_item."DisplayOrder"
        FROM scoped_unit unit
        CROSS JOIN source_scope source
        INNER JOIN "asset"."UnitPackages" unit_package
            ON unit_package."PropertyId" = unit."PropertyId"
           AND unit_package."IsDeleted" = FALSE
           AND (
                (source."HasRoomOverride" = TRUE AND unit_package."UnitId" = unit."Id")
                OR (source."HasRoomOverride" = FALSE AND unit_package."UnitId" IS NULL)
           )
        INNER JOIN "masterdata"."MasterDataValues" package_status
            ON package_status."Id" = unit_package."StatusId"
           AND package_status."Code" = @ActiveStatusCode
           AND package_status."IsDeleted" = FALSE
           AND package_status."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" package_type
            ON package_type."Id" = unit_package."PackageTypeId"
           AND package_type."IsDeleted" = FALSE
           AND package_type."IsActive" = TRUE
        LEFT JOIN "asset"."UnitPackageItems" package_item
            ON package_item."UnitPackageId" = unit_package."Id"
           AND package_item."IsDeleted" = FALSE
        WHERE package_type."Code" <> @NoFurniturePackageTypeCode
        ORDER BY unit_package."PackageName", package_item."DisplayOrder" NULLS LAST, package_item."ItemName";
        """;

    /// <summary>
    /// Counts protected dependency rows across one property graph before delete.
    /// </summary>
    public const string GET_PROPERTY_MUTATION_GUARD_QUERY = """
        WITH scoped_property AS (
            SELECT property."Id"
            FROM "asset"."Properties" property
            WHERE property."PublicId" = @PropertyPublicId
              AND property."IsDeleted" = FALSE
            LIMIT 1
        ),
        scoped_units AS (
            SELECT unit."Id"
            FROM "asset"."Units" unit
            INNER JOIN scoped_property property
                ON property."Id" = unit."PropertyId"
            WHERE unit."IsDeleted" = FALSE
        )
        SELECT
            (
                SELECT COUNT(contract."Id")::int
                FROM "leasing"."Contracts" contract
                WHERE contract."UnitId" IN (SELECT "Id" FROM scoped_units)
                  AND contract."IsDeleted" = FALSE
            ) AS "ContractCount",
            (
                SELECT COUNT(occupancy."Id")::int
                FROM "leasing"."Occupancies" occupancy
                INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                    ON occupancy_status."Id" = occupancy."StatusId"
                   AND occupancy_status."IsDeleted" = FALSE
                   AND occupancy_status."IsActive" = TRUE
                   AND occupancy_status."Code" = 'ACTIVE'
                WHERE occupancy."UnitId" IN (SELECT "Id" FROM scoped_units)
                  AND occupancy."IsDeleted" = FALSE
            ) AS "OccupancyCount",
            (
                SELECT COUNT(DISTINCT invoice."Id")::int
                FROM "billing"."Invoices" invoice
                INNER JOIN "leasing"."Contracts" contract
                    ON contract."Id" = invoice."ContractId"
                   AND contract."IsDeleted" = FALSE
                WHERE contract."UnitId" IN (SELECT "Id" FROM scoped_units)
                  AND invoice."IsDeleted" = FALSE
            ) + (
                SELECT COUNT(invoice_line."Id")::int
                FROM "billing"."InvoiceLines" invoice_line
                INNER JOIN "billing"."Invoices" invoice
                    ON invoice."Id" = invoice_line."InvoiceId"
                   AND invoice."IsDeleted" = FALSE
                INNER JOIN "leasing"."Contracts" contract
                    ON contract."Id" = invoice."ContractId"
                   AND contract."IsDeleted" = FALSE
                WHERE contract."UnitId" IN (SELECT "Id" FROM scoped_units)
                  AND invoice_line."IsDeleted" = FALSE
            ) AS "InvoiceCount",
            (
                SELECT COUNT(vehicle."Id")::int
                FROM "core"."PartyVehicles" vehicle
                WHERE vehicle."UnitId" IN (SELECT "Id" FROM scoped_units)
                  AND vehicle."IsDeleted" = FALSE
            ) AS "VehicleCount",
            (
                SELECT COUNT(document_link."Id")::int
                FROM "core"."DocumentLinks" document_link
                INNER JOIN "masterdata"."MasterDataValues" entity_type
                    ON entity_type."Id" = document_link."EntityTypeId"
                   AND entity_type."IsDeleted" = FALSE
                   AND entity_type."IsActive" = TRUE
                WHERE document_link."IsDeleted" = FALSE
                  AND (
                        (entity_type."Code" = 'PROPERTY' AND document_link."EntityId" IN (SELECT "Id" FROM scoped_property))
                        OR (entity_type."Code" = 'UNIT' AND document_link."EntityId" IN (SELECT "Id" FROM scoped_units))
                      )
            ) AS "DocumentLinkCount";
        """;

    /// <summary>
    /// Counts protected dependency rows for selected room public identifiers.
    /// </summary>
    public const string GET_UNIT_MUTATION_GUARDS_QUERY = """
        WITH scoped_units AS (
            SELECT unit."Id", unit."PublicId"
            FROM "asset"."Units" unit
            WHERE unit."PublicId" = ANY(@UnitPublicIds)
              AND unit."IsDeleted" = FALSE
        )
        SELECT
            unit."PublicId" AS "UnitPublicId",
            (
                SELECT COUNT(contract."Id")::int
                FROM "leasing"."Contracts" contract
                WHERE contract."UnitId" = unit."Id"
                  AND contract."IsDeleted" = FALSE
            ) AS "ContractCount",
            (
                SELECT COUNT(occupancy."Id")::int
                FROM "leasing"."Occupancies" occupancy
                INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                    ON occupancy_status."Id" = occupancy."StatusId"
                   AND occupancy_status."IsDeleted" = FALSE
                   AND occupancy_status."IsActive" = TRUE
                   AND occupancy_status."Code" = 'ACTIVE'
                WHERE occupancy."UnitId" = unit."Id"
                  AND occupancy."IsDeleted" = FALSE
            ) AS "OccupancyCount",
            (
                SELECT COUNT(DISTINCT invoice."Id")::int
                FROM "billing"."Invoices" invoice
                INNER JOIN "leasing"."Contracts" contract
                    ON contract."Id" = invoice."ContractId"
                   AND contract."IsDeleted" = FALSE
                WHERE contract."UnitId" = unit."Id"
                  AND invoice."IsDeleted" = FALSE
            ) + (
                SELECT COUNT(invoice_line."Id")::int
                FROM "billing"."InvoiceLines" invoice_line
                INNER JOIN "billing"."Invoices" invoice
                    ON invoice."Id" = invoice_line."InvoiceId"
                   AND invoice."IsDeleted" = FALSE
                INNER JOIN "leasing"."Contracts" contract
                    ON contract."Id" = invoice."ContractId"
                   AND contract."IsDeleted" = FALSE
                WHERE contract."UnitId" = unit."Id"
                  AND invoice_line."IsDeleted" = FALSE
            ) AS "InvoiceCount",
            (
                SELECT COUNT(vehicle."Id")::int
                FROM "core"."PartyVehicles" vehicle
                WHERE vehicle."UnitId" = unit."Id"
                  AND vehicle."IsDeleted" = FALSE
            ) AS "VehicleCount",
            (
                SELECT COUNT(document_link."Id")::int
                FROM "core"."DocumentLinks" document_link
                INNER JOIN "masterdata"."MasterDataValues" entity_type
                    ON entity_type."Id" = document_link."EntityTypeId"
                   AND entity_type."IsDeleted" = FALSE
                   AND entity_type."IsActive" = TRUE
                   AND entity_type."Code" = 'UNIT'
                WHERE document_link."EntityId" = unit."Id"
                  AND document_link."IsDeleted" = FALSE
            ) AS "DocumentLinkCount"
        FROM scoped_units unit;
        """;

    /// <summary>
    /// Counts invoice-line references for selected property-level charge policies.
    /// </summary>
    public const string GET_CHARGE_POLICY_MUTATION_GUARDS_QUERY = """
        SELECT
            policy."PublicId" AS "PolicyPublicId",
            COUNT(invoice_line."Id")::int AS "InvoiceLineCount"
        FROM "asset"."RentalChargePolicies" policy
        LEFT JOIN "billing"."InvoiceLines" invoice_line
            ON invoice_line."ChargePolicyId" = policy."Id"
           AND invoice_line."IsDeleted" = FALSE
        WHERE policy."PublicId" = ANY(@PolicyPublicIds)
          AND policy."IsDeleted" = FALSE
        GROUP BY policy."PublicId";
        """;

    /// <summary>
    /// Counts contract references for selected package templates under one property.
    /// </summary>
    public const string GET_PACKAGE_TEMPLATE_MUTATION_GUARDS_QUERY = """
        SELECT
            unit_package."PublicId" AS "PackagePublicId",
            COUNT(contract."Id")::int AS "ContractCount"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."UnitPackages" unit_package
            ON unit_package."PropertyId" = property."Id"
           AND unit_package."UnitId" IS NULL
           AND unit_package."IsDeleted" = FALSE
           AND unit_package."PublicId" = ANY(@PackagePublicIds)
        LEFT JOIN "leasing"."Contracts" contract
            ON contract."UnitPackageId" = unit_package."Id"
           AND contract."IsDeleted" = FALSE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE
        GROUP BY unit_package."PublicId";
        """;

    /// <summary>
    /// Resolves one room and its property inside the current landlord scope.
    /// </summary>
    public const string GET_METER_SCOPE_QUERY = """
        SELECT property."Id" AS "PropertyId", property."PublicId" AS "PropertyPublicId",
               property."PropertyName", unit."Id" AS "UnitId", unit."PublicId" AS "RoomPublicId",
               unit."UnitName" AS "RoomName"
        FROM "asset"."Units" unit
        INNER JOIN "asset"."Properties" property
          ON property."Id" = unit."PropertyId" AND property."IsDeleted" = FALSE
        WHERE unit."PublicId" = @RoomPublicId AND unit."IsDeleted" = FALSE
          AND EXISTS (
              SELECT 1 FROM "asset"."PropertyParties" property_party
              INNER JOIN "masterdata"."MasterDataValues" relationship_type
                ON relationship_type."Id" = property_party."RelationshipTypeId"
               AND relationship_type."Code" = ANY(@RelationshipCodes)
               AND relationship_type."IsDeleted" = FALSE AND relationship_type."IsActive" = TRUE
              WHERE property_party."PropertyId" = property."Id"
                AND property_party."PartyId" = @CurrentPartyId
                AND property_party."IsDeleted" = FALSE
                AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE))
        LIMIT 1;
        """;

    /// <summary>
    /// Resolves and locks one room inside the current landlord scope for meter mutation.
    /// </summary>
    public const string GET_METER_MUTATION_SCOPE_ID_FOR_UPDATE_QUERY = GET_SCOPED_ROOM_ID_QUERY + "\nFOR UPDATE";

    /// <summary>
    /// Locks the latest active invoice for one room and billing period.
    /// </summary>
    public const string GET_INVOICE_ID_FOR_UPDATE_QUERY = """
        SELECT invoice."Id" AS "Value"
        FROM "billing"."Invoices" invoice
        INNER JOIN "leasing"."Contracts" contract ON contract."Id" = invoice."ContractId"
        WHERE contract."UnitId" = @UnitId
          AND invoice."BillingPeriodFrom" = @PeriodFrom
          AND invoice."BillingPeriodTo" = @PeriodTo
          AND invoice."IsDeleted" = FALSE
        ORDER BY invoice."Id" DESC
        LIMIT 1
        FOR UPDATE;
        """;

    /// <summary>
    /// Checks whether an invoice has an allocation backed by a successful payment.
    /// </summary>
    public const string HAS_SUCCESSFUL_INVOICE_PAYMENT_QUERY = """
        SELECT EXISTS (
            SELECT 1 FROM "billing"."PaymentAllocations" allocation
            INNER JOIN "billing"."Payments" payment ON payment."Id" = allocation."PaymentId"
            INNER JOIN "masterdata"."MasterDataValues" payment_status
                ON payment_status."Id" = payment."StatusId"
               AND payment_status."Code" = @SuccessfulPaymentStatusCode
               AND payment_status."IsDeleted" = FALSE
               AND payment_status."IsActive" = TRUE
            INNER JOIN "masterdata"."MasterDataTypes" payment_status_type
                ON payment_status_type."Id" = payment_status."MasterDataTypeId"
               AND payment_status_type."Code" = @PaymentStatusTypeCode
               AND payment_status_type."IsDeleted" = FALSE
               AND payment_status_type."IsActive" = TRUE
            WHERE allocation."InvoiceId" = @InvoiceId
              AND allocation."IsDeleted" = FALSE
              AND payment."IsDeleted" = FALSE
        );
        """;

    /// <summary>
    /// Loads current and previous meter records for one scoped room.
    /// </summary>
    public const string GET_METER_PERIOD_ROWS_QUERY = """
        SELECT property."PublicId" AS "PropertyPublicId", property."PropertyName",
               unit."PublicId" AS "RoomPublicId", unit."UnitName" AS "RoomName",
               reading."PublicId" AS "MeterPublicId", reading."Id" AS "MeterId",
               line_type."Code" AS "LineTypeCode", reading."BillingPeriodFrom", reading."ReadingDate",
               reading."PreviousReading", reading."CurrentReading", reading."UsageQuantity",
               reading."UnitPriceSnapshot", status."Code" AS "StatusCode",
               CASE WHEN policy."IsUsageBased" THEN 'METER_READING' ELSE 'FIXED' END AS "CalculationMethodCode",
               policy."Amount" AS "SuggestedUnitPrice",
               EXISTS (
                   SELECT 1
                   FROM "billing"."Meters" prior_reading
                   INNER JOIN "masterdata"."MasterDataValues" prior_status
                     ON prior_status."Id" = prior_reading."StatusId"
                    AND prior_status."Code" = @ConfirmedMeterStatusCode
                   WHERE prior_reading."PropertyId" = reading."PropertyId"
                     AND prior_reading."UnitId" = reading."UnitId"
                     AND prior_reading."LineTypeId" = reading."LineTypeId"
                     AND prior_reading."BillingPeriodFrom" < reading."BillingPeriodFrom"
                     AND prior_reading."IsDeleted" = FALSE
               ) AS "HasPriorConfirmedReading"
        FROM "billing"."Meters" reading
        INNER JOIN "asset"."Properties" property ON property."Id" = reading."PropertyId" AND property."IsDeleted" = FALSE
        LEFT JOIN "asset"."Units" unit ON unit."Id" = reading."UnitId" AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" line_type ON line_type."Id" = reading."LineTypeId"
        INNER JOIN "masterdata"."MasterDataValues" status ON status."Id" = reading."StatusId"
        LEFT JOIN "asset"."RentalChargePolicies" policy ON policy."Id" = reading."ChargePolicyId" AND policy."IsDeleted" = FALSE
        WHERE reading."PropertyId" = @PropertyId
          AND reading."UnitId" = @UnitId
          AND reading."BillingPeriodFrom" IN (@PeriodFrom, @PreviousPeriodFrom)
          AND reading."IsDeleted" = FALSE
          AND EXISTS (
              SELECT 1 FROM "asset"."PropertyParties" property_party
              INNER JOIN "masterdata"."MasterDataValues" relationship_type
                ON relationship_type."Id" = property_party."RelationshipTypeId"
               AND relationship_type."Code" = ANY(@RelationshipCodes)
               AND relationship_type."IsDeleted" = FALSE AND relationship_type."IsActive" = TRUE
              WHERE property_party."PropertyId" = property."Id"
                AND property_party."PartyId" = @CurrentPartyId
                AND property_party."IsDeleted" = FALSE
                AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE))
        ORDER BY reading."BillingPeriodFrom" DESC, line_type."Code";
        """;

    /// <summary>
    /// Loads at most twelve room meter periods for one calendar year.
    /// </summary>
    public const string GET_METER_HISTORY_ROWS_QUERY = """
        WITH selected_periods AS (
            SELECT reading."BillingPeriodFrom"
            FROM "billing"."Meters" reading
            WHERE reading."PropertyId" = @PropertyId
              AND reading."UnitId" = @UnitId
              AND reading."BillingPeriodFrom" BETWEEN @HistoryFrom AND @HistoryTo
              AND reading."IsDeleted" = FALSE
            GROUP BY reading."BillingPeriodFrom"
            ORDER BY "BillingPeriodFrom" DESC
            LIMIT 12
        )
        SELECT property."PublicId" AS "PropertyPublicId", property."PropertyName",
               unit."PublicId" AS "RoomPublicId", unit."UnitName" AS "RoomName",
               reading."PublicId" AS "MeterPublicId", reading."Id" AS "MeterId",
               line_type."Code" AS "LineTypeCode", reading."BillingPeriodFrom", reading."ReadingDate",
               reading."PreviousReading", reading."CurrentReading", reading."UsageQuantity",
               reading."UnitPriceSnapshot", status."Code" AS "StatusCode",
               invoice_status."Code" AS "InvoiceStatusCode", invoice."PaidAmount" AS "InvoicePaidAmount",
               EXISTS (
                   SELECT 1
                   FROM "billing"."Meters" prior_reading
                   INNER JOIN "masterdata"."MasterDataValues" prior_status
                     ON prior_status."Id" = prior_reading."StatusId"
                    AND prior_status."Code" = @ConfirmedMeterStatusCode
                   WHERE prior_reading."PropertyId" = reading."PropertyId"
                     AND prior_reading."UnitId" = reading."UnitId"
                     AND prior_reading."LineTypeId" = reading."LineTypeId"
                     AND prior_reading."BillingPeriodFrom" < reading."BillingPeriodFrom"
                     AND prior_reading."IsDeleted" = FALSE
               ) AS "HasPriorConfirmedReading",
               EXISTS (
                   SELECT 1 FROM "billing"."PaymentAllocations" allocation
                   INNER JOIN "billing"."Payments" payment ON payment."Id" = allocation."PaymentId"
                   WHERE allocation."InvoiceId" = invoice."Id"
                     AND allocation."IsDeleted" = FALSE AND payment."IsDeleted" = FALSE
               ) AS "HasInvoicePayment"
        FROM selected_periods
        INNER JOIN "billing"."Meters" reading
          ON reading."PropertyId" = @PropertyId
         AND reading."UnitId" = @UnitId
         AND reading."BillingPeriodFrom" = selected_periods."BillingPeriodFrom"
         AND reading."IsDeleted" = FALSE
        INNER JOIN "asset"."Properties" property ON property."Id" = reading."PropertyId" AND property."IsDeleted" = FALSE
        LEFT JOIN "asset"."Units" unit ON unit."Id" = reading."UnitId" AND unit."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" line_type ON line_type."Id" = reading."LineTypeId"
        INNER JOIN "masterdata"."MasterDataValues" status ON status."Id" = reading."StatusId"
        LEFT JOIN LATERAL (
            SELECT candidate.*
            FROM "billing"."Invoices" candidate
            INNER JOIN "leasing"."Contracts" contract ON contract."Id" = candidate."ContractId"
            WHERE contract."UnitId" = @UnitId
              AND candidate."BillingPeriodFrom" = (reading."BillingPeriodFrom" + INTERVAL '1 month')::date
              AND candidate."BillingPeriodTo" = (reading."BillingPeriodFrom" + INTERVAL '2 months' - INTERVAL '1 day')::date
              AND candidate."IsDeleted" = FALSE
            ORDER BY candidate."Id" DESC
            LIMIT 1
        ) invoice ON TRUE
        LEFT JOIN "masterdata"."MasterDataValues" invoice_status ON invoice_status."Id" = invoice."StatusId"
        WHERE EXISTS (
              SELECT 1 FROM "asset"."PropertyParties" property_party
              INNER JOIN "masterdata"."MasterDataValues" relationship_type
                ON relationship_type."Id" = property_party."RelationshipTypeId"
               AND relationship_type."Code" = ANY(@RelationshipCodes)
               AND relationship_type."IsDeleted" = FALSE AND relationship_type."IsActive" = TRUE
              WHERE property_party."PropertyId" = property."Id"
                AND property_party."PartyId" = @CurrentPartyId
                AND property_party."IsDeleted" = FALSE
                AND (property_party."StartDate" IS NULL OR property_party."StartDate" <= CURRENT_DATE)
                AND (property_party."EndDate" IS NULL OR property_party."EndDate" >= CURRENT_DATE))
        ORDER BY reading."BillingPeriodFrom" DESC, line_type."Code";
        """;

    /// <summary>
    /// Loads evidence metadata for selected meter records.
    /// </summary>
    public const string GET_METER_EVIDENCE_QUERY = """
        SELECT document."PublicId" AS "EvidencePublicId", link."EntityId" AS "MeterId",
               document."FileUrl" AS "Url", document."ContentType", link."SortOrder"
        FROM "core"."DocumentLinks" link
        INNER JOIN "core"."Documents" document ON document."Id" = link."DocumentId" AND document."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" entity_type ON entity_type."Id" = link."EntityTypeId" AND entity_type."Code" = 'METER'
        INNER JOIN "masterdata"."MasterDataValues" link_type ON link_type."Id" = link."LinkTypeId" AND link_type."Code" = 'METER_PHOTO'
        WHERE link."EntityId" = ANY(@MeterIds) AND link."IsDeleted" = FALSE
        ORDER BY link."EntityId", link."SortOrder";
        """;

    /// <summary>
    /// Loads the next invoice state for one room meter period.
    /// </summary>
    public const string GET_METER_INVOICE_IMPACT_QUERY = """
        SELECT
            invoice."PublicId" AS "InvoicePublicId",
            status."Code" AS "StatusCode",
            invoice."PaidAmount",
            EXISTS (
                SELECT 1
                FROM "billing"."PaymentAllocations" allocation
                INNER JOIN "billing"."Payments" payment
                    ON payment."Id" = allocation."PaymentId"
                   AND payment."IsDeleted" = FALSE
                INNER JOIN "masterdata"."MasterDataValues" payment_status
                    ON payment_status."Id" = payment."StatusId"
                   AND payment_status."Code" = @SuccessfulPaymentStatusCode
                   AND payment_status."IsDeleted" = FALSE
                   AND payment_status."IsActive" = TRUE
                INNER JOIN "masterdata"."MasterDataTypes" payment_status_type
                    ON payment_status_type."Id" = payment_status."MasterDataTypeId"
                   AND payment_status_type."Code" = @PaymentStatusTypeCode
                   AND payment_status_type."IsDeleted" = FALSE
                   AND payment_status_type."IsActive" = TRUE
                WHERE allocation."InvoiceId" = invoice."Id"
                  AND allocation."IsDeleted" = FALSE
            ) AS "HasSuccessfulPayment"
        FROM "billing"."Invoices" invoice
        INNER JOIN "leasing"."Contracts" contract ON contract."Id" = invoice."ContractId"
        INNER JOIN "masterdata"."MasterDataValues" status ON status."Id" = invoice."StatusId"
        WHERE contract."UnitId" = @UnitId
          AND invoice."BillingPeriodFrom" = @PeriodFrom
          AND invoice."BillingPeriodTo" = @PeriodTo
          AND invoice."IsDeleted" = FALSE
        ORDER BY invoice."Id" DESC
        LIMIT 1;
        """;
}
