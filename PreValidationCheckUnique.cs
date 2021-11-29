using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginIncrementNumber
{
    public class PreValidationCheckUnique : IPlugin
    {
		private bool IsUnique(EntityCollection collection, Guid currentId)
		{
			if(collection.Entities.Count == 0)
            {
				return true;
            }

			var foundId = collection.Entities[0].Id;
			if(foundId == currentId)
            {
				return true;
			}

			return false;
		}

		private EntityCollection FindEntitiesWithSameObjective(IOrganizationService organizationService, OptionSetValue objectiveValue)
		{
			ConditionExpression condition1 = new ConditionExpression();
			condition1.AttributeName = "mtx_objectivecode";
			condition1.Operator = ConditionOperator.Equal;
			condition1.Values.Add(objectiveValue.Value);

			FilterExpression filter1 = new FilterExpression();
			filter1.Conditions.Add(condition1);

			QueryExpression query = new QueryExpression(mtx_IncrementNumber.EntityLogicalName);
			query.ColumnSet.AddColumns("mtx_objectivecode");
			query.Criteria.AddFilter(filter1);

			return organizationService.RetrieveMultiple(query);
		}


		public void Execute(IServiceProvider serviceProvider)
        {
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && ((Entity)context.InputParameters["Target"]).LogicalName.Equals(mtx_IncrementNumber.EntityLogicalName))
			{
				mtx_IncrementNumber incrementNumber = ((Entity)context.InputParameters["Target"]).ToEntity<mtx_IncrementNumber>(); ;

				IOrganizationServiceFactory organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
				IOrganizationService organizationService = organizationServiceFactory.CreateOrganizationService(context.UserId);


				OptionSetValue objectiveValue = (OptionSetValue) incrementNumber.mtx_ObjectiveCode;
				EntityCollection entitiesWithSameObjective = FindEntitiesWithSameObjective(organizationService, objectiveValue);

                if (IsUnique(entitiesWithSameObjective, incrementNumber.Id) == false)
                {
                    throw new InvalidPluginExecutionException("The given objective is already occupied");
                }
            }
		}
    }
}
