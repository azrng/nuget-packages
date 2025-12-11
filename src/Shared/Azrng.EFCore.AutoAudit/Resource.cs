using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Azrng.EFCore.AutoAudit;

internal class Resource
{
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    //
    // 摘要:
    //     Returns the cached ResourceManager instance used by this class.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
        get
        {
            if (resourceMan == null)
            {
                resourceMan = new ResourceManager("Azrng.EFCore.AutoAudit.Resource", typeof(Resource).Assembly);
            }

            return resourceMan;
        }
    }

    //
    // 摘要:
    //     Looks up a localized string similar to {0} must be lambda expression.
    internal static string propertyExpression_must_be_lambda_expression =>
        ResourceManager.GetString("propertyExpression_must_be_lambda_expression", resourceCulture);
}