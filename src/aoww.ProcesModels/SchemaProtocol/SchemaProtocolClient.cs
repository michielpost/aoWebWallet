using aoww.ProcesModels.Metadata;
using aoww.ProcesModels.SchemaProtocol.Extensions;
using aoww.ProcesModels.SchemaProtocol.Models;
using ArweaveAO;
using ArweaveAO.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels.SchemaProtocol
{
    public class SchemaProtocolClient(AODataClient aoDataClient)
    {
        public async Task<List<ActionMetadata>> GetSchemaProtocolActions(string processId)
        {
            try
            {
                var result = await aoDataClient.DryRun(processId, new ArweaveAO.Requests.DryRunRequest
                {
                    Target = processId,
                    Tags = new List<ArweaveAO.Models.Tag>
                    {
                        new ArweaveAO.Models.Tag() { Name = "Action", Value = "Schema" }
                    }
                });

                var data = result?.GetFirstDataValue();

                if (data != null)
                {
                    var model = System.Text.Json.JsonSerializer.Deserialize<SchemaProtocolModel>(data);

                    if (model != null)
                    {
                        List<ActionMetadata> actionList = model.Select(x => x.ToActionMetadata(processId)).Where(x => x != null).Select(x => x!).ToList();
                        return actionList;
                    }
                }
            }
            catch { }

            return new();
        }
    }
}
