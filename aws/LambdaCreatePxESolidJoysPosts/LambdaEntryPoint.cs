using Amazon.Lambda.Core;
using LambdaCore.Helper;
using LambdaCore.Services;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PxELambda
{
    public class LambdaEntryPoint
    {
        public string Run(object input, ILambdaContext context)
        {
            string result;

            try
            {
                Console.Write("Creating Solid Joys Entries...");

                AsyncHelper.RunSync(() => new SolidJoysUpdater().UpdateSolidJoysEntries());

                result = "Process completed successfully";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }
    }
}
