namespace Haven.Infrastructure.Constants;

/// <summary>
/// Stores SQL text for Haven business optimized reads and lookup operations.
/// </summary>
public static class InfrastructureQueryConstants
{
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
    /// Loads the current authenticated user's selected party context.
    /// </summary>
    public const string GET_CURRENT_PARTY_CONTEXT_QUERY = """
        SELECT
            party."Id" AS "PartyId",
            party."PublicId" AS "PartyPublicId",
            party."DisplayName",
            party_type."Code" AS "PartyTypeCode",
            party_status."Code" AS "StatusCode"
        FROM "identity"."Users" "user"
        INNER JOIN "core"."Parties" party
            ON party."Id" = "user"."CurrentPartyId"
           AND party."IsDeleted" = FALSE
        INNER JOIN "masterdata"."MasterDataValues" party_type
            ON party_type."Id" = party."PartyTypeId"
           AND party_type."IsDeleted" = FALSE
           AND party_type."IsActive" = TRUE
        INNER JOIN "masterdata"."MasterDataValues" party_status
            ON party_status."Id" = party."StatusId"
           AND party_status."IsDeleted" = FALSE
           AND party_status."IsActive" = TRUE
        WHERE "user"."PublicId" = @UserPublicId
          AND "user"."IsDeleted" = FALSE
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
            property."Description",
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
            thumbnail."ThumbnailUrl",
            COALESCE(NULLIF(property."TotalFloors", 0), summary."TotalFloors", 0)::int AS "TotalFloors",
            COALESCE(NULLIF(property."TotalUnits", 0), summary."TotalUnits", 0)::int AS "TotalUnits",
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
        LEFT JOIN LATERAL (
            SELECT document."FileUrl" AS "ThumbnailUrl"
            FROM "core"."DocumentLinks" document_link
            INNER JOIN "core"."Documents" document
                ON document."Id" = document_link."DocumentId"
               AND document."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" entity_type
                ON entity_type."Id" = document_link."EntityTypeId"
               AND entity_type."IsDeleted" = FALSE
               AND entity_type."IsActive" = TRUE
               AND entity_type."Code" = 'PROPERTY'
            INNER JOIN "masterdata"."MasterDataValues" link_type
                ON link_type."Id" = document_link."LinkTypeId"
               AND link_type."IsDeleted" = FALSE
               AND link_type."IsActive" = TRUE
               AND link_type."Code" = 'PROPERTY_IMAGE'
            WHERE document_link."EntityId" = property."Id"
              AND document_link."IsDeleted" = FALSE
            ORDER BY document_link."IsPrimary" DESC, document_link."SortOrder", document_link."CreatedAt"
            LIMIT 1
        ) thumbnail ON TRUE
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
            thumbnail."ThumbnailUrl",
            COALESCE(NULLIF(property."TotalFloors", 0), summary."TotalFloors", 0)::int AS "TotalFloors",
            COALESCE(NULLIF(property."TotalUnits", 0), summary."TotalUnits", 0)::int AS "TotalUnits",
            COALESCE(summary."AvailableUnitCount", 0)::int AS "AvailableUnitCount",
            COALESCE(summary."OccupiedUnitCount", 0)::int AS "OccupiedUnitCount",
            COALESCE(summary."MaintenanceUnitCount", 0)::int AS "MaintenanceUnitCount",
            COALESCE(summary."PublishedUnitCount", 0)::int AS "PublishedUnitCount"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."PropertyParties" property_party
            ON property_party."PropertyId" = property."Id"
           AND property_party."IsDeleted" = FALSE
           AND property_party."PartyId" = @CurrentPartyId
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
        LEFT JOIN LATERAL (
            SELECT document."FileUrl" AS "ThumbnailUrl"
            FROM "core"."DocumentLinks" document_link
            INNER JOIN "core"."Documents" document
                ON document."Id" = document_link."DocumentId"
               AND document."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" entity_type
                ON entity_type."Id" = document_link."EntityTypeId"
               AND entity_type."IsDeleted" = FALSE
               AND entity_type."IsActive" = TRUE
               AND entity_type."Code" = 'PROPERTY'
            INNER JOIN "masterdata"."MasterDataValues" link_type
                ON link_type."Id" = document_link."LinkTypeId"
               AND link_type."IsDeleted" = FALSE
               AND link_type."IsActive" = TRUE
               AND link_type."Code" = 'PROPERTY_IMAGE'
            WHERE document_link."EntityId" = property."Id"
              AND document_link."IsDeleted" = FALSE
            ORDER BY document_link."IsPrimary" DESC, document_link."SortOrder", document_link."CreatedAt"
            LIMIT 1
        ) thumbnail ON TRUE
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
            COALESCE(occupancy_count."OccupiedBedCount", 0)::int AS "OccupiedBedCount"
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
            tenant_party."PublicId" AS "TenantPublicId",
            tenant_party."DisplayName" AS "TenantName",
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
           AND occupancy."IsPrimaryTenant" = TRUE
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
            unit_package."PackageCode",
            unit_package."PackageName",
            unit_package."PriceAdjustment",
            package_item."ItemName",
            package_item."DisplayOrder"
        FROM "asset"."Properties" property
        INNER JOIN "asset"."Units" unit
            ON unit."PropertyId" = property."Id"
           AND unit."IsDeleted" = FALSE
        INNER JOIN "asset"."UnitPackages" unit_package
            ON unit_package."UnitId" = unit."Id"
           AND unit_package."IsDeleted" = FALSE
        LEFT JOIN "asset"."UnitPackageItems" package_item
            ON package_item."UnitPackageId" = unit_package."Id"
           AND package_item."IsDeleted" = FALSE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE
        ORDER BY unit_package."PackageCode", package_item."DisplayOrder" NULLS LAST, package_item."ItemName";
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
    /// Loads management counters for the property detail hub.
    /// </summary>
    public const string GET_PROPERTY_MANAGEMENT_SUMMARY_QUERY = """
        SELECT
            COALESCE(tenant_summary."TenantCount", 0)::int AS "TenantCount",
            COALESCE(contract_summary."ActiveContractCount", 0)::int AS "ActiveContractCount",
            COALESCE(document_summary."DocumentCount", 0)::int AS "DocumentCount",
            COALESCE(invoice_summary."UnpaidInvoiceCount", 0)::int AS "UnpaidInvoiceCount",
            COALESCE(invoice_summary."OverdueInvoiceCount", 0)::int AS "OverdueInvoiceCount"
        FROM "asset"."Properties" property
        LEFT JOIN LATERAL (
            SELECT COUNT(DISTINCT occupancy."PartyId")::int AS "TenantCount"
            FROM "asset"."Units" unit
            INNER JOIN "leasing"."Occupancies" occupancy
                ON occupancy."UnitId" = unit."Id"
               AND occupancy."IsDeleted" = FALSE
               AND occupancy."IsPrimaryTenant" = TRUE
            INNER JOIN "masterdata"."MasterDataValues" occupancy_status
                ON occupancy_status."Id" = occupancy."StatusId"
               AND occupancy_status."IsDeleted" = FALSE
               AND occupancy_status."IsActive" = TRUE
               AND occupancy_status."Code" = 'ACTIVE'
            WHERE unit."PropertyId" = property."Id"
              AND unit."IsDeleted" = FALSE
        ) tenant_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(contract."Id")::int AS "ActiveContractCount"
            FROM "asset"."Units" unit
            INNER JOIN "leasing"."Contracts" contract
                ON contract."UnitId" = unit."Id"
               AND contract."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" contract_status
                ON contract_status."Id" = contract."StatusId"
               AND contract_status."IsDeleted" = FALSE
               AND contract_status."IsActive" = TRUE
               AND contract_status."Code" = 'ACTIVE'
            WHERE unit."PropertyId" = property."Id"
              AND unit."IsDeleted" = FALSE
              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
        ) contract_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT COUNT(document_link."Id")::int AS "DocumentCount"
            FROM "masterdata"."MasterDataValues" entity_type
            INNER JOIN "core"."DocumentLinks" document_link
                ON document_link."EntityTypeId" = entity_type."Id"
               AND document_link."EntityId" = property."Id"
               AND document_link."IsDeleted" = FALSE
            WHERE entity_type."IsDeleted" = FALSE
              AND entity_type."IsActive" = TRUE
              AND entity_type."Code" = 'PROPERTY'
        ) document_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT
                COUNT(invoice."Id") FILTER (
                    WHERE invoice_status."Code" NOT IN ('PAID', 'CANCELLED')
                )::int AS "UnpaidInvoiceCount",
                COUNT(invoice."Id") FILTER (
                    WHERE invoice."DueDate" < CURRENT_DATE
                      AND invoice_status."Code" NOT IN ('PAID', 'CANCELLED')
                )::int AS "OverdueInvoiceCount"
            FROM "asset"."Units" unit
            INNER JOIN "leasing"."Contracts" contract
                ON contract."UnitId" = unit."Id"
               AND contract."IsDeleted" = FALSE
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
            WHERE unit."PropertyId" = property."Id"
              AND unit."IsDeleted" = FALSE
              AND (contract."EffectiveFrom" IS NULL OR contract."EffectiveFrom" <= CURRENT_DATE)
              AND (contract."EffectiveTo" IS NULL OR contract."EffectiveTo" >= CURRENT_DATE)
        ) invoice_summary ON TRUE
        WHERE property."PublicId" = @PropertyPublicId
          AND property."IsDeleted" = FALSE;
        """;
}
