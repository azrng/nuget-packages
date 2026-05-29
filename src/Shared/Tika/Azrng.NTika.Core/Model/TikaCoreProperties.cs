using System;

namespace Azrng.NTika.Core.Model
{
    public static class TikaCoreProperties
    {
        public static readonly Property RESOURCE_NAME_KEY = Property.InternalText("resourceName");
        public static readonly Property EMBEDDED_ID = Property.InternalText("embeddedId");
        public static readonly Property EMBEDDED_DEPTH = Property.InternalIntegerSequence("embeddedDepth");
        public static readonly Property EMBEDDED_RESOURCE_PATH = Property.InternalText("embeddedResourcePath");

        public static readonly Property CONTENT_TYPE = Property.InternalText("Content-Type");
        public static readonly Property CONTENT_TYPE_HINT = Property.InternalText("Content-Type-Hint");
        public static readonly Property CONTENT_TYPE_USER_OVERRIDE = Property.InternalText("Content-Type-User-Override");
        public static readonly Property CONTENT_TYPE_PARSER_OVERRIDE = Property.InternalText("Content-Type-Parser-Override");
        public static readonly Property CONTENT_TYPE_MAGIC_DETECTED = Property.InternalText("Content-Type-Magic-Detected");

        public static readonly Property TIKA_PARSED_BY = Property.InternalText("X-TIKA:Parsed-By");
        public static readonly Property TIKA_PARSED_BY_FULL_SET = Property.InternalTextBag("X-TIKA:Parsed-By-Full-Set");
        public static readonly Property TIKA_CONTENT = Property.InternalText("X-TIKA:content");
        public static readonly Property TIKA_DETECTED_LANGUAGE = Property.InternalText("X-TIKA:detectedLanguage");

        // Dublin Core
        public static readonly Property TITLE = Property.ExternalText("dc:title");
        public static readonly Property CREATOR = Property.ExternalText("dc:creator");
        public static readonly Property DESCRIPTION = Property.ExternalText("dc:description");
        public static readonly Property SUBJECT = Property.ExternalText("dc:subject");
        public static readonly Property CREATED = Property.ExternalDate("dcterms:created");
        public static readonly Property MODIFIED = Property.ExternalDate("dcterms:modified");
        public static readonly Property FORMAT = Property.ExternalText("dc:format");
        public static readonly Property IDENTIFIER = Property.ExternalText("dc:identifier");
        public static readonly Property LANGUAGE = Property.ExternalText("dc:language");
        public static readonly Property RELATION = Property.ExternalText("dc:relation");
        public static readonly Property SOURCE = Property.ExternalText("dc:source");
        public static readonly Property TYPE = Property.ExternalText("dc:type");
        public static readonly Property PUBLISHER = Property.ExternalText("dc:publisher");
        public static readonly Property CONTRIBUTOR = Property.ExternalText("dc:contributor");
        public static readonly Property RIGHTS = Property.ExternalText("dc:rights");

        // Geographic
        public static readonly Property LATITUDE = Property.ExternalReal("geo:lat");
        public static readonly Property LONGITUDE = Property.ExternalReal("geo:long");
        public static readonly Property ALTITUDE = Property.ExternalReal("geo:alt");

        public enum EmbeddedResourceType
        {
            INLINE,
            ATTACHMENT,
            MACRO,
            METADATA,
            FONT,
            THUMBNAIL,
            RENDERING,
            VERSION
        }
    }
}
