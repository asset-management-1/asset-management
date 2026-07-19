namespace Haven.Application.Constants;

/// <summary>
/// Stores Haven property module constants shared by commands, queries, and services.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Default page number for property listing.
    /// </summary>
    public const int DEFAULT_PAGE_NUMBER = 1;

    /// <summary>
    /// Default page size for property listing.
    /// </summary>
    public const int DEFAULT_PROPERTY_PAGE_SIZE = 20;

    /// <summary>
    /// Maximum page size accepted by property listing.
    /// </summary>
    public const int MAX_PROPERTY_PAGE_SIZE = 100;

    /// <summary>
    /// Default page size for room listing.
    /// </summary>
    public const int DEFAULT_ROOM_PAGE_SIZE = 20;

    /// <summary>
    /// Maximum page size accepted by room listing.
    /// </summary>
    public const int MAX_ROOM_PAGE_SIZE = 100;

    /// <summary>
    /// Maximum number of floor rows accepted in one property structure payload.
    /// </summary>
    public const int MAX_PROPERTY_STRUCTURE_FLOORS = 200;

    /// <summary>
    /// Maximum number of room rows accepted under one floor in a property structure payload.
    /// </summary>
    public const int MAX_PROPERTY_STRUCTURE_ROOMS_PER_FLOOR = 100;

    /// <summary>
    /// Maximum total room rows accepted in a single property structure payload.
    /// </summary>
    public const int MAX_PROPERTY_STRUCTURE_ROOMS = 1000;

    /// <summary>
    /// Maximum charge policy rows accepted in one property or room edit payload.
    /// </summary>
    public const int MAX_CHARGE_POLICIES = 100;

    /// <summary>
    /// Maximum package templates accepted in one property or room edit payload.
    /// </summary>
    public const int MAX_PACKAGE_TEMPLATES = 100;

    /// <summary>
    /// Maximum package item rows accepted in one package template.
    /// </summary>
    public const int MAX_PACKAGE_ITEMS = 100;

    /// <summary>
    /// Landlord party type code.
    /// </summary>
    public const string MASTER_CODE_PARTY_TYPE_LANDLORD = "LANDLORD";

    /// <summary>
    /// Tenant party type code.
    /// </summary>
    public const string MASTER_CODE_PARTY_TYPE_TENANT = "TENANT";

    /// <summary>
    /// Active party/common status code.
    /// </summary>
    public const string MASTER_CODE_ACTIVE = "ACTIVE";

    /// <summary>
    /// Draft property status used for newly created properties.
    /// </summary>
    public const string MASTER_CODE_PROPERTY_STATUS_DRAFT = "DRAFT";

    /// <summary>
    /// Available unit status assigned to newly created rooms.
    /// </summary>
    public const string MASTER_CODE_UNIT_STATUS_AVAILABLE = "AVAILABLE";

    /// <summary>
    /// Occupied unit status used when a room has active occupancies.
    /// </summary>
    public const string MASTER_CODE_UNIT_STATUS_OCCUPIED = "OCCUPIED";

    /// <summary>
    /// Rental contract type code.
    /// </summary>
    public const string MASTER_CODE_CONTRACT_TYPE_RENTAL = "RENTAL";

    /// <summary>
    /// Offline-upload contract source code used for landlord-created contracts.
    /// </summary>
    public const string MASTER_CODE_CONTRACT_SOURCE_OFFLINE_UPLOAD = "OFFLINE_UPLOAD";

    /// <summary>
    /// Active contract status code.
    /// </summary>
    public const string MASTER_CODE_CONTRACT_STATUS_ACTIVE = "ACTIVE";

    /// <summary>
    /// Active occupancy status code.
    /// </summary>
    public const string MASTER_CODE_OCCUPANCY_STATUS_ACTIVE = "ACTIVE";

    /// <summary>
    /// Pending occupancy status code used when a tenant joins by room QR before landlord contract setup.
    /// </summary>
    public const string MASTER_CODE_OCCUPANCY_STATUS_PENDING = "PENDING";

    /// <summary>
    /// Moved-out occupancy status code.
    /// </summary>
    public const string MASTER_CODE_OCCUPANCY_STATUS_MOVED_OUT = "MOVED_OUT";

    /// <summary>
    /// Default package type used when a property has no selected service or furniture package.
    /// </summary>
    public const string MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE = "NO_FURNITURE";

    /// <summary>
    /// Custom package type used for service/furniture packages defined during property setup.
    /// </summary>
    public const string MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM = "CUSTOM";

    /// <summary>
    /// Active package status used for created default property packages.
    /// </summary>
    public const string MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE = "ACTIVE";

    /// <summary>
    /// Invoice line type code used for parking charge policies.
    /// </summary>
    public const string MASTER_CODE_INVOICE_LINE_TYPE_PARKING = "PARKING";

    /// <summary>
    /// Unit type master-data code for a normal rentable room.
    /// </summary>
    public const string MASTER_CODE_UNIT_TYPE_ROOM = "ROOM";

    /// <summary>
    /// Rental mode master-data code for renting the whole room/unit.
    /// </summary>
    public const string MASTER_CODE_RENTAL_MODE_WHOLE_UNIT = "WHOLE_UNIT";

    /// <summary>
    /// Rental mode code used when several primary tenants rent beds in one room.
    /// </summary>
    public const string MASTER_CODE_RENTAL_MODE_SHARED_BED = "SHARED_BED";

    /// <summary>
    /// Property relationship codes allowed to access landlord-facing property reads.
    /// </summary>
    public static readonly IReadOnlyCollection<string> PROPERTY_ACCESS_RELATIONSHIP_CODES =
        Enum.GetValues<PropertyRelationshipTypeEnum>()
            .Select(x => x.ToMasterDataCode())
            .ToArray();

    /// <summary>
    /// Calculation-method code that maps to usage-based charge policies.
    /// </summary>
    public const string CALCULATION_METHOD_METER_READING = "METER_READING";

    /// <summary>
    /// Calculation-method code that maps to fixed charge policies.
    /// </summary>
    public const string CALCULATION_METHOD_FIXED = "FIXED";

    /// <summary>
    /// Invoice-line type code for electricity charges.
    /// </summary>
    public const string MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC = "ELECTRIC";

    /// <summary>
    /// Invoice-line type code for water charges.
    /// </summary>
    public const string MASTER_CODE_INVOICE_LINE_TYPE_WATER = "WATER";

    /// <summary>
    /// Meter status code for a submitted reading.
    /// </summary>
    public const string MASTER_CODE_METER_STATUS_SUBMITTED = "SUBMITTED";

    /// <summary>
    /// Meter status code for a confirmed reading.
    /// </summary>
    public const string MASTER_CODE_METER_STATUS_CONFIRMED = "CONFIRMED";

    /// <summary>
    /// Utility charge-mode code for meter readings.
    /// </summary>
    public const string MASTER_CODE_UTILITY_CHARGE_MODE_METER_READING = "METER_READING";

    /// <summary>
    /// Invoice status code for a draft invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_DRAFT = "DRAFT";

    /// <summary>
    /// Invoice status code for an issued invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_ISSUED = "ISSUED";

    /// <summary>
    /// Invoice status code for an overdue invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_OVERDUE = "OVERDUE";

    /// <summary>
    /// Invoice status code for a partially paid invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID = "PARTIALLY_PAID";

    /// <summary>
    /// Invoice status code for a paid invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_PAID = "PAID";

    /// <summary>
    /// Invoice status code for a cancelled invoice.
    /// </summary>
    public const string MASTER_CODE_INVOICE_STATUS_CANCELLED = "CANCELLED";

    /// <summary>
    /// Document type code for a meter evidence photo.
    /// </summary>
    public const string MASTER_CODE_DOCUMENT_TYPE_METER_PHOTO = "METER_PHOTO";

    /// <summary>
    /// Storage-provider code for Cloudflare R2.
    /// </summary>
    public const string MASTER_CODE_STORAGE_PROVIDER_R2 = "R2";

    /// <summary>
    /// Active document status code.
    /// </summary>
    public const string MASTER_CODE_DOCUMENT_STATUS_ACTIVE = "ACTIVE";

    /// <summary>
    /// Entity-type code used to link evidence to a meter.
    /// </summary>
    public const string MASTER_CODE_ENTITY_TYPE_METER = "METER";

    /// <summary>
    /// Document-link type code for meter evidence.
    /// </summary>
    public const string MASTER_CODE_DOCUMENT_LINK_TYPE_METER_PHOTO = "METER_PHOTO";

    /// <summary>
    /// Active document-link status code.
    /// </summary>
    public const string MASTER_CODE_DOCUMENT_LINK_STATUS_ACTIVE = "ACTIVE";

    /// <summary>
    /// Payment-status code that identifies a successfully completed payment.
    /// </summary>
    public const string MASTER_CODE_PAYMENT_STATUS_SUCCESS = "SUCCESS";

    /// <summary>
    /// Description stored on generated utility invoice lines.
    /// </summary>
    public const string UTILITY_INVOICE_LINE_DESCRIPTION = "Utility meter reading";

    /// <summary>
    /// Prefix used for generated invoice codes.
    /// </summary>
    public const string INVOICE_CODE_PREFIX = "INV";

    /// <summary>
    /// Number of random digits used in generated invoice codes.
    /// </summary>
    public const int INVOICE_CODE_RANDOM_LENGTH = 12;

    /// <summary>
    /// Previous-reading source code for a manually supplied baseline.
    /// </summary>
    public const string UTILITY_PREVIOUS_SOURCE_MANUAL = "MANUAL_BASELINE";

    /// <summary>
    /// Previous-reading source code for the latest confirmed period.
    /// </summary>
    public const string UTILITY_PREVIOUS_SOURCE_PRIOR_CONFIRMED = "PRIOR_CONFIRMED";

    /// <summary>
    /// Invoice-impact action used when no invoice change is required.
    /// </summary>
    public const string UTILITY_INVOICE_ACTION_NONE = "NONE";

    /// <summary>
    /// Invoice-impact action used while waiting for invoice creation.
    /// </summary>
    public const string UTILITY_INVOICE_ACTION_WAITING = "WAITING_FOR_BILL";

    /// <summary>
    /// Invoice-impact action used when a draft invoice can be updated.
    /// </summary>
    public const string UTILITY_INVOICE_ACTION_UPDATE_DRAFT = "UPDATE_DRAFT";

    /// <summary>
    /// Invoice-impact action used when an issued invoice needs replacement.
    /// </summary>
    public const string UTILITY_INVOICE_ACTION_REPLACE_ISSUED = "REPLACE_ISSUED";

    /// <summary>
    /// Invoice-impact action used when payment blocks meter mutation.
    /// </summary>
    public const string UTILITY_INVOICE_ACTION_BLOCKED_PAID = "BLOCKED_PAID";

    /// <summary>
    /// Distributed-lock key prefix for room meter mutations.
    /// </summary>
    public const string METER_LOCK_KEY_PREFIX = "haven:meter:";

    /// <summary>
    /// Initial meter mutation lock lease in seconds.
    /// </summary>
    public const int METER_LOCK_LEASE_SECONDS = 30;

    /// <summary>
    /// Composite slot format for meter type and evidence position.
    /// </summary>
    public const string METER_EVIDENCE_SLOT_FORMAT = "{0}-{1}";

    /// <summary>
    /// Prefix used for auto-generated property codes.
    /// </summary>
    public const string PROPERTY_CODE_PREFIX = "PROP";

    /// <summary>
    /// Number of numeric random digits kept in generated property codes.
    /// </summary>
    public const int PROPERTY_CODE_RANDOM_LENGTH = 10;

    /// <summary>
    /// Maximum attempts for generating an unused property code.
    /// </summary>
    public const int PROPERTY_CODE_MAX_ATTEMPTS = 5;

    /// <summary>
    /// Prefix used for backend-owned room/unit business codes.
    /// </summary>
    public const string ROOM_CODE_PREFIX = "ROOM";

    /// <summary>
    /// Number of numeric random digits kept in generated room/unit business codes.
    /// </summary>
    public const int ROOM_CODE_RANDOM_LENGTH = 10;

    /// <summary>
    /// Maximum attempts for generating an unused room/unit business code.
    /// </summary>
    public const int ROOM_CODE_MAX_ATTEMPTS = 5;

    /// <summary>
    /// Number of days a property stays recoverable after delete is requested.
    /// </summary>
    public const int PROPERTY_PENDING_DELETE_DAYS = 7;

    /// <summary>
    /// Prefix used for internal package option codes.
    /// </summary>
    public const string UNIT_PACKAGE_CODE_PREFIX = "PKG";

    /// <summary>
    /// Number of numeric random digits kept in generated package codes.
    /// </summary>
    public const int UNIT_PACKAGE_CODE_RANDOM_LENGTH = 6;

    /// <summary>
    /// Maximum collision retries used when generating one room package code.
    /// </summary>
    public const int UNIT_PACKAGE_CODE_MAX_ATTEMPTS = 5;

    /// <summary>
    /// Room update mode that removes room-level overrides and falls back to common property setup.
    /// </summary>
    public const string ROOM_OVERRIDE_MODE_COMMON = "COMMON";

    /// <summary>
    /// Room update mode that applies submitted room-level override data.
    /// </summary>
    public const string ROOM_OVERRIDE_MODE_CUSTOM = "CUSTOM";

    /// <summary>
    /// Tenant role code for a contract-signing primary tenant.
    /// </summary>
    public const string TENANT_ROLE_PRIMARY = "PRIMARY";

    /// <summary>
    /// Tenant role code for a non-contract occupant/member.
    /// </summary>
    public const string TENANT_ROLE_OCCUPANT = "OCCUPANT";

    /// <summary>
    /// Prefix used for generated rental contract codes.
    /// </summary>
    public const string CONTRACT_CODE_PREFIX = "CTR";

    /// <summary>
    /// Initial numeric random length used for rental contract codes.
    /// </summary>
    public const int CONTRACT_CODE_RANDOM_LENGTH = 10;

    /// <summary>
    /// Maximum attempts for each contract-code segment length.
    /// </summary>
    public const int CONTRACT_CODE_MAX_ATTEMPTS = 5;

    /// <summary>
    /// Room tenant join QR token lifetime in minutes.
    /// </summary>
    public const int TENANT_JOIN_TOKEN_EXPIRY_MINUTES = 5;

    /// <summary>
    /// Maximum accepted QR join token length.
    /// </summary>
    public const int TENANT_JOIN_TOKEN_MAX_LENGTH = 100;

    /// <summary>
    /// Cache key prefix used for room tenant QR join payloads.
    /// </summary>
    public const string TENANT_JOIN_CACHE_KEY_PREFIX = "haven:tenant-join:";

    /// <summary>
    /// Distributed-lock key prefix used to serialize occupancy mutations for one room.
    /// </summary>
    public const string ROOM_OCCUPANCY_LOCK_KEY_PREFIX = "haven:room:";

    /// <summary>
    /// Distributed-lock key suffix that separates occupancy coordination from other room operations.
    /// </summary>
    public const string ROOM_OCCUPANCY_LOCK_KEY_SUFFIX = ":occupancy";

    /// <summary>
    /// Initial room occupancy lock lease in seconds.
    /// </summary>
    public const int ROOM_OCCUPANCY_LOCK_LEASE_SECONDS = 30;

    /// <summary>
    /// Logical object-storage slot for the vehicle registration front image.
    /// </summary>
    public const string VEHICLE_REGISTRATION_FRONT_IMAGE_SLOT = "registration-front";

    /// <summary>
    /// Logical object-storage slot for the vehicle registration side image.
    /// </summary>
    public const string VEHICLE_REGISTRATION_SIDE_IMAGE_SLOT = "registration-side";

    /// <summary>
    /// Logical object-storage slot for the vehicle front image.
    /// </summary>
    public const string VEHICLE_FRONT_IMAGE_SLOT = "vehicle-front";

    /// <summary>
    /// Logical object-storage slot for the vehicle side image.
    /// </summary>
    public const string VEHICLE_SIDE_IMAGE_SLOT = "vehicle-side";

    /// <summary>
    /// Location kind label used when resolving province codes.
    /// </summary>
    public const string LOCATION_KIND_PROVINCE = "province";

    /// <summary>
    /// Location kind label used when resolving district codes.
    /// </summary>
    public const string LOCATION_KIND_DISTRICT = "district";

    /// <summary>
    /// Location kind label used when resolving ward codes.
    /// </summary>
    public const string LOCATION_KIND_WARD = "ward";

}
