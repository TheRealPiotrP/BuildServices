using System;

namespace Microsoft.Its.Recipes
{
    internal static partial class Any
    {
        internal static partial class MyGet
        {
            internal static partial class WebHooks
            {
                internal static object PingPayload()
                {
                    return BasePayload();
                }

                internal static object PackageAddedPayload(Uri packageDownloadUrl = null)
                {
                    return new
                    {
                        Identifier = Any.Guid(),
                        Username = Any.Word(),
                        When = Any.DateTimeOffset(),
                        PayloadType = "PackageAddedWebHookEventPayloadV1",
                            Payload = new { 
                            PackageType = "NuGet",
                            PackageIdentifier = Any.Word(),
                            PackageVersion = Any.Word(),
                            PackageDetailsUrl = Any.Uri(),
                            PackageDownloadUrl = packageDownloadUrl ?? Any.Uri(),
                            PackageMetadata = new
                            {
                                IconUrl = Any.Uri(),
                                Size = Any.Int(1),
                                Authors = Any.FullName(),
                                Description = Any.Paragraph(),
                                LicenseUrl = Any.Uri(),
                                LicenseNames = Any.Word(),
                                ProjectUrl = Any.Uri(),
                                Tags = Any.Paragraph(),
                                Dependencies = new[]
                                {
                                    new
                                    {
                                        PackageIdentifier = Any.Word(),
                                        PackageVersion = "(>= 2.0.5364.25176)",
                                        TargetFramework = ".NETFramework,Version=v4.0.0.0"
                                    }
                                }
                            }
                        }
                    };
                }

                private static object BasePayload()
                {
                    return new
                    {
                        Identifier = Any.Guid(),
                        Username = Any.Word(),
                        When = Any.DateTimeOffset(),
                        PayloadType = Any.String(),
                        Payload = new { }
                    };
                }
            }
        }
    }
}
