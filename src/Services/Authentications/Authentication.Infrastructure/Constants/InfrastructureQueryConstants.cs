namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Stores database script constants used by the authentication infrastructure layer.
/// Keep raw SQL text and stored-procedure names here so repositories stay focused on mapping.
/// </summary>
public static class InfrastructureQueryConstants
{
    /// <summary>
    /// Locking query that finds one refresh-session identifier by its current or immediate previous token hash.
    /// </summary>
    public const string GET_REFRESH_TOKEN_ID_FOR_UPDATE_QUERY = """
        SELECT refresh_token."Id"
        FROM "identity"."RefreshTokens" refresh_token
        WHERE (refresh_token."TokenHash" = @RefreshTokenHash
               OR refresh_token."PreviousTokenHash" = @RefreshTokenHash)
          AND refresh_token."IsDeleted" = FALSE
        FOR UPDATE OF refresh_token
        """;

    /// <summary>
    /// Locking query that loads one active refresh-session identifier by user and public session identifiers.
    /// </summary>
    public const string GET_ACTIVE_REFRESH_TOKEN_ID_BY_SESSION_FOR_UPDATE_QUERY = """
        SELECT refresh_token."Id"
        FROM "identity"."RefreshTokens" refresh_token
        WHERE refresh_token."UserId" = @UserId
          AND refresh_token."SessionPublicId" = @SessionPublicId
          AND refresh_token."IsDeleted" = FALSE
          AND refresh_token."RevokedAt" IS NULL
          AND refresh_token."ExpiresAt" > CURRENT_TIMESTAMP
        FOR UPDATE OF refresh_token
        """;

    /// <summary>
    /// Locking query that loads one non-deleted user identifier by public identifier.
    /// </summary>
    public const string GET_USER_ID_BY_PUBLIC_ID_FOR_UPDATE_QUERY = """
        SELECT user_row."Id"
        FROM "identity"."Users" user_row
        WHERE user_row."PublicId" = @UserPublicId
          AND user_row."IsDeleted" = FALSE
        FOR UPDATE OF user_row
        """;

    /// <summary>
    /// Locking query that loads one non-deleted password-identity user identifier by normalised email.
    /// </summary>
    public const string GET_PASSWORD_IDENTITY_USER_ID_BY_EMAIL_FOR_UPDATE_QUERY = """
        SELECT user_row."Id"
        FROM "identity"."Users" user_row
        WHERE user_row."Email" = @NormalizedEmail
          AND user_row."IsDeleted" = FALSE
        FOR UPDATE OF user_row
        """;

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
                current_session."CurrentPartyId",
                u."FullName",
                u."UserName",
                u."Email",
                u."PhoneNumber",
                u."AvatarUrl",
                u."DateOfBirth",
                gender."Code" AS "Gender",
                current_party."DisplayName" AS "DisplayName",
                LOWER(current_party_type."Code") AS "CurrentContext"
            FROM "identity"."Users" u
            INNER JOIN "identity"."RefreshTokens" current_session
                ON current_session."UserId" = u."Id"
               AND current_session."SessionPublicId" = @SessionPublicId
               AND current_session."RevokedAt" IS NULL
               AND current_session."ExpiresAt" > NOW()
               AND current_session."IsDeleted" = FALSE
            INNER JOIN "identity"."UserParties" current_user_party
                ON current_user_party."UserId" = u."Id"
               AND current_user_party."PartyId" = current_session."CurrentPartyId"
               AND current_user_party."IsDeleted" = FALSE
            INNER JOIN "core"."Parties" current_party
                ON current_party."Id" = current_user_party."PartyId"
               AND current_party."IsDeleted" = FALSE
            INNER JOIN "masterdata"."MasterDataValues" current_party_status
                ON current_party_status."Id" = current_party."StatusId"
               AND current_party_status."IsDeleted" = FALSE
               AND current_party_status."IsActive" = TRUE
               AND current_party_status."Code" = 'ACTIVE'
            INNER JOIN "masterdata"."MasterDataTypes" current_party_status_type
                ON current_party_status_type."Id" = current_party_status."MasterDataTypeId"
               AND current_party_status_type."IsDeleted" = FALSE
               AND current_party_status_type."Code" = 'PartyStatus'
            INNER JOIN "masterdata"."MasterDataValues" current_party_type
                ON current_party_type."Id" = current_party."PartyTypeId"
               AND current_party_type."IsDeleted" = FALSE
               AND current_party_type."IsActive" = TRUE
            INNER JOIN "masterdata"."MasterDataTypes" current_party_type_definition
                ON current_party_type_definition."Id" = current_party_type."MasterDataTypeId"
               AND current_party_type_definition."IsDeleted" = FALSE
               AND current_party_type_definition."Code" = 'PartyType'
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
                    WHERE master_type."Code" = 'IdentifierType'
                      AND master_value."Code" = 'CCCD'
                ) AS "CccdIdentifierTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE master_type."Code" = 'IdentifierType'
                      AND master_value."Code" = 'PASSPORT'
                ) AS "PassportIdentifierTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE master_type."Code" = 'EntityType'
                      AND master_value."Code" = 'PARTY'
                ) AS "PartyEntityTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE master_type."Code" = 'DocumentLinkType'
                      AND master_value."Code" = 'NATIONAL_ID_SCAN'
                ) AS "SingleScanLinkTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE master_type."Code" = 'DocumentLinkType'
                      AND master_value."Code" = 'NATIONAL_ID_FRONT_SCAN'
                ) AS "FrontLinkTypeId",
                MAX(master_value."Id") FILTER (
                    WHERE master_type."Code" = 'DocumentLinkType'
                      AND master_value."Code" = 'NATIONAL_ID_BACK_SCAN'
                ) AS "BackLinkTypeId"
            FROM "masterdata"."MasterDataTypes" master_type
            INNER JOIN "masterdata"."MasterDataValues" master_value
                ON master_value."MasterDataTypeId" = master_type."Id"
               AND master_value."IsDeleted" = FALSE
               AND master_value."IsActive" = TRUE
            WHERE master_type."IsDeleted" = FALSE
              AND master_type."Code" IN (
                  'IdentifierType',
                  'EntityType',
                  'DocumentLinkType'
              )
              AND master_value."Code" IN (
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
                kyc_status."Code" AS "Status",
                identifier_type."Code" AS "IdentifierType",
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
                    WHEN kyc_status."Code" IN ('PENDING', 'APPROVED') THEN 0
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
                    LOWER(party_type."Code") AS "Value"
                FROM "identity"."UserParties" user_party
                INNER JOIN "core"."Parties" party
                    ON party."Id" = user_party."PartyId"
                   AND party."IsDeleted" = FALSE
                INNER JOIN "masterdata"."MasterDataValues" party_status
                    ON party_status."Id" = party."StatusId"
                   AND party_status."IsDeleted" = FALSE
                   AND party_status."IsActive" = TRUE
                   AND party_status."Code" = 'ACTIVE'
                INNER JOIN "masterdata"."MasterDataTypes" party_status_type
                    ON party_status_type."Id" = party_status."MasterDataTypeId"
                   AND party_status_type."IsDeleted" = FALSE
                   AND party_status_type."Code" = 'PartyStatus'
                INNER JOIN "masterdata"."MasterDataValues" party_type
                    ON party_type."Id" = party."PartyTypeId"
                   AND party_type."IsDeleted" = FALSE
                   AND party_type."IsActive" = TRUE
                INNER JOIN "masterdata"."MasterDataTypes" party_type_definition
                    ON party_type_definition."Id" = party_type."MasterDataTypeId"
                   AND party_type_definition."IsDeleted" = FALSE
                   AND party_type_definition."Code" = 'PartyType'
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
