namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Stores database script constants used by the authentication infrastructure layer.
/// Keep raw SQL text and stored-procedure names here so repositories stay focused on mapping.
/// </summary>
public static class InfrastructureQueryConstants
{
    /// <summary>
    /// Batch query that resolves multiple active master-data values by requested type/value pairs.
    /// </summary>
    public const string GET_MASTER_DATA_VALUES_BY_TYPE_AND_VALUE_QUERY = """
        WITH requested AS (
            SELECT
                request."Ordinal",
                request."Type" AS "KeyType",
                request."Value" AS "KeyValue"
            FROM UNNEST(@KeyTypes, @KeyValues, @KeyOrdinals) AS request("Type", "Value", "Ordinal")
            WHERE request."Type" IS NOT NULL
              AND request."Type" <> ''
              AND request."Value" IS NOT NULL
              AND request."Value" <> ''
        )
        SELECT DISTINCT ON (requested."Ordinal")
            requested."KeyType",
            requested."KeyValue",
            master_value."Id",
            master_value."MasterDataTypeId",
            master_value."Code",
            master_value."Name"
        FROM requested
        INNER JOIN "masterdata"."MasterDataTypes" master_type
            ON master_type."IsDeleted" = FALSE
           AND master_type."Code" = requested."KeyType"
        INNER JOIN "masterdata"."MasterDataValues" master_value
            ON master_value."MasterDataTypeId" = master_type."Id"
           AND master_value."IsDeleted" = FALSE
           AND master_value."IsActive" = TRUE
           AND master_value."Code" = requested."KeyValue"
        ORDER BY
            requested."Ordinal",
            master_value."Id";
        """;

    /// <summary>
    /// Single-row query that loads current-user profile data plus JSON-projected account collections
    /// by user public identifier.
    /// </summary>
    public const string GET_USER_INFO_RESPONSE_BY_PUBLIC_ID_QUERY = """
        WITH current_user_profile AS MATERIALIZED (
            SELECT
                u."Id" AS "UserId",
                u."CurrentPartyId",
                u."FullName",
                u."UserName",
                u."Email",
                u."PhoneNumber",
                u."AvatarUrl",
                u."DateOfBirth",
                COALESCE(NULLIF(gender."Code", ''), gender."Name") AS "Gender",
                current_party."DisplayName" AS "DisplayName",
                LOWER(COALESCE(NULLIF(current_party_type."Code", ''), current_party_type."Name")) AS "CurrentContext"
            FROM "identity"."Users" u
            LEFT JOIN "core"."Parties" current_party
                ON current_party."Id" = u."CurrentPartyId"
               AND current_party."IsDeleted" = FALSE
            LEFT JOIN "masterdata"."MasterDataValues" current_party_type
                ON current_party_type."Id" = current_party."PartyTypeId"
               AND current_party_type."IsDeleted" = FALSE
               AND current_party_type."IsActive" = TRUE
            LEFT JOIN "masterdata"."MasterDataValues" gender
                ON gender."Id" = u."GenderId"
               AND gender."IsDeleted" = FALSE
               AND gender."IsActive" = TRUE
            WHERE u."PublicId" = @UserPublicId
              AND u."IsDeleted" = FALSE
        ),
        lookup_values AS MATERIALIZED (
            SELECT
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'IdentifierType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'CCCD'
                ) AS "CccdIdentifierTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'IdentifierType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'PASSPORT'
                ) AS "PassportIdentifierTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'EntityType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'PARTY'
                ) AS "PartyEntityTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'DocumentLinkType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'NATIONAL_ID_SCAN'
                ) AS "SingleScanLinkTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'DocumentLinkType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'NATIONAL_ID_FRONT_SCAN'
                ) AS "FrontLinkTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE COALESCE(NULLIF(master_type."Code", ''), master_type."Name") = 'DocumentLinkType'
                      AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") = 'NATIONAL_ID_BACK_SCAN'
                ) AS "BackLinkTypeId"
            FROM "masterdata"."MasterDataTypes" master_type
            INNER JOIN "masterdata"."MasterDataValues" master_value
                ON master_value."MasterDataTypeId" = master_type."Id"
               AND master_value."IsDeleted" = FALSE
               AND master_value."IsActive" = TRUE
            WHERE master_type."IsDeleted" = FALSE
              AND COALESCE(NULLIF(master_type."Code", ''), master_type."Name") IN (
                  'IdentifierType',
                  'EntityType',
                  'DocumentLinkType'
              )
              AND COALESCE(NULLIF(master_value."Code", ''), master_value."Name") IN (
                  'CCCD',
                  'PASSPORT',
                  'PARTY',
                  'NATIONAL_ID_SCAN',
                  'NATIONAL_ID_FRONT_SCAN',
                  'NATIONAL_ID_BACK_SCAN'
              )
        ),
        linked_parties AS MATERIALIZED (
            SELECT DISTINCT user_party."PartyId"
            FROM current_user_profile
            INNER JOIN "identity"."UserParties" user_party
                ON user_party."UserId" = current_user_profile."UserId"
               AND user_party."IsDeleted" = FALSE
            UNION
            SELECT current_user_profile."CurrentPartyId"
            FROM current_user_profile
            WHERE current_user_profile."CurrentPartyId" IS NOT NULL
        )
        SELECT
            current_user_profile."FullName",
            current_user_profile."UserName",
            current_user_profile."Email",
            current_user_profile."PhoneNumber",
            current_user_profile."AvatarUrl",
            current_user_profile."DateOfBirth",
            current_user_profile."Gender",
            current_user_profile."DisplayName",
            current_user_profile."CurrentContext",
            COALESCE(kyc_summary."IsSubmitted", FALSE) AS "KycIsSubmitted",
            kyc_summary."Status" AS "KycStatus",
            kyc_summary."IdentifierType" AS "KycIdentifierType",
            kyc_summary."IdentifierTypeDisplayName" AS "KycIdentifierTypeDisplayName",
            kyc_summary."MaskedIdentifier" AS "KycMaskedIdentifier",
            COALESCE(kyc_summary."HasFrontFile", FALSE) AS "KycHasFrontFile",
            COALESCE(kyc_summary."HasBackFile", FALSE) AS "KycHasBackFile",
            COALESCE(available_contexts."JsonValue", '[]') AS "AvailableContextsJson",
            COALESCE(external_providers."JsonValue", '[]') AS "ExternalProvidersJson"
        FROM current_user_profile
        CROSS JOIN lookup_values
        LEFT JOIN LATERAL (
            SELECT
                TRUE AS "IsSubmitted",
                COALESCE(NULLIF(kyc_status."Code", ''), kyc_status."Name") AS "Status",
                COALESCE(NULLIF(identifier_type."Code", ''), identifier_type."Name") AS "IdentifierType",
                identifier_type."Name" AS "IdentifierTypeDisplayName",
                CASE
                    WHEN party_identifier."IdentifierValue" IS NULL THEN NULL
                    WHEN LENGTH(party_identifier."IdentifierValue") <= 4 THEN REPEAT('*', LENGTH(party_identifier."IdentifierValue"))
                    ELSE CONCAT(REPEAT('*', LENGTH(party_identifier."IdentifierValue") - 4), RIGHT(party_identifier."IdentifierValue", 4))
                END AS "MaskedIdentifier",
                EXISTS (
                    SELECT 1
                    FROM "core"."DocumentLinks" document_link
                    INNER JOIN "core"."Documents" document
                        ON document."Id" = document_link."DocumentId"
                       AND document."IsDeleted" = FALSE
                    WHERE document_link."EntityTypeId" = lookup_values."PartyEntityTypeId"
                      AND document_link."EntityId" IN (SELECT "PartyId" FROM linked_parties)
                      AND (
                          (
                              party_identifier."IdentifierTypeId" = lookup_values."CccdIdentifierTypeId"
                              AND document_link."LinkTypeId" = lookup_values."FrontLinkTypeId"
                          )
                          OR (
                              party_identifier."IdentifierTypeId" = lookup_values."PassportIdentifierTypeId"
                              AND document_link."LinkTypeId" = lookup_values."SingleScanLinkTypeId"
                          )
                      )
                      AND document_link."IsDeleted" = FALSE
                ) AS "HasFrontFile",
                EXISTS (
                    SELECT 1
                    FROM "core"."DocumentLinks" document_link
                    INNER JOIN "core"."Documents" document
                        ON document."Id" = document_link."DocumentId"
                       AND document."IsDeleted" = FALSE
                    WHERE document_link."EntityTypeId" = lookup_values."PartyEntityTypeId"
                      AND document_link."EntityId" IN (SELECT "PartyId" FROM linked_parties)
                      AND party_identifier."IdentifierTypeId" = lookup_values."CccdIdentifierTypeId"
                      AND document_link."LinkTypeId" = lookup_values."BackLinkTypeId"
                      AND document_link."IsDeleted" = FALSE
                ) AS "HasBackFile"
            FROM "core"."PartyIdentifiers" party_identifier
            INNER JOIN linked_parties
                ON linked_parties."PartyId" = party_identifier."PartyId"
            INNER JOIN "masterdata"."MasterDataValues" kyc_status
                ON kyc_status."Id" = party_identifier."StatusId"
               AND kyc_status."IsDeleted" = FALSE
               AND kyc_status."IsActive" = TRUE
            INNER JOIN "masterdata"."MasterDataValues" identifier_type
                ON identifier_type."Id" = party_identifier."IdentifierTypeId"
               AND identifier_type."IsDeleted" = FALSE
               AND identifier_type."IsActive" = TRUE
            WHERE party_identifier."IdentifierTypeId" IN (
                  lookup_values."CccdIdentifierTypeId",
                  lookup_values."PassportIdentifierTypeId"
              )
              AND party_identifier."IsDeleted" = FALSE
            ORDER BY
                CASE
                    WHEN COALESCE(NULLIF(kyc_status."Code", ''), kyc_status."Name") IN ('PENDING', 'APPROVED') THEN 0
                    ELSE 1
                END,
                CASE WHEN party_identifier."PartyId" = current_user_profile."CurrentPartyId" THEN 0 ELSE 1 END,
                party_identifier."UpdatedAt" DESC NULLS LAST,
                party_identifier."CreatedAt" DESC
            LIMIT 1
        ) kyc_summary ON TRUE
        LEFT JOIN LATERAL (
            SELECT COALESCE(jsonb_agg(context_value."Value" ORDER BY context_value."Value"), '[]'::jsonb)::text AS "JsonValue"
            FROM (
                SELECT DISTINCT
                    LOWER(COALESCE(NULLIF(party_type."Code", ''), party_type."Name")) AS "Value"
                FROM "identity"."UserParties" user_party
                INNER JOIN "core"."Parties" party
                    ON party."Id" = user_party."PartyId"
                   AND party."IsDeleted" = FALSE
                INNER JOIN "masterdata"."MasterDataValues" party_type
                    ON party_type."Id" = party."PartyTypeId"
                   AND party_type."IsDeleted" = FALSE
                   AND party_type."IsActive" = TRUE
                WHERE user_party."UserId" = current_user_profile."UserId"
                  AND user_party."IsDeleted" = FALSE
            ) context_value
        ) available_contexts ON TRUE
        LEFT JOIN LATERAL (
            SELECT COALESCE(
                jsonb_agg(
                    jsonb_build_object(
                        'Provider', provider_value."Provider",
                        'IsLinked', TRUE
                    )
                    ORDER BY provider_value."Provider"
                ),
                '[]'::jsonb
            )::text AS "JsonValue"
            FROM (
                SELECT DISTINCT external_login."LoginProvider" AS "Provider"
                FROM "identity"."ExternalLogins" external_login
                WHERE external_login."UserId" = current_user_profile."UserId"
                  AND external_login."IsDeleted" = FALSE
            ) provider_value
        ) external_providers ON TRUE
        """;
}
