﻿#if !(XRM_MOCKUP_2011 || XRM_MOCKUP_2013 || XRM_MOCKUP_2015)
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Metadata;
using DG.Tools.XrmMockup.Database;

namespace DG.Tools.XrmMockup
{
    internal class CloseIncidentRequestHandler : RequestHandler
    {
        internal CloseIncidentRequestHandler(Core core, XrmDb db, MetadataSkeleton metadata, Security security) : base(core, db, metadata, security, "CloseIncident") { }

        internal override OrganizationResponse Execute(OrganizationRequest orgRequest, EntityReference userRef)
        {
            var request = MakeRequest<CloseIncidentRequest>(orgRequest);

            if (request.IncidentResolution == null)
            {
                throw new FaultException("Required field 'IncidentResolution' is missing");
            }

            if (request.IncidentResolution.LogicalName == null)
            {
                throw new FaultException("Required member 'LogicalName' missing for field 'IncidentResolution'");
            }

            if (request.IncidentResolution.LogicalName != "incidentresolution")
            {
                throw new FaultException($"The entity with a name = '{request.IncidentResolution.LogicalName}' was not found in the MetadataCache.");
            }

            if (request.Status == null)
            {
                throw new FaultException("Required field 'Status' is missing");
            }

            var defaultStateStatuses = metadata.DefaultStateStatus
                .Where(s => s.Key == "incident")
                .FirstOrDefault();

            if (!defaultStateStatuses.Value.Any((dss => (dss.Key == 1 && dss.Value == request.Status.Value))))
            {
                throw new FaultException($"{request.Status} is not a valid status code on incident with Id {request.IncidentResolution.Id}.");
            }

            var incidentRef = request.IncidentResolution.GetAttributeValue<EntityReference>("incidentid");

            if (incidentRef == null)
            {
                throw new FaultException("The incident id is missing.");
            }

            var incident = db.GetDbRowOrNull(incidentRef);

            if (incident == null)
            {
                throw new FaultException($"incident With Id = {incidentRef.Id} Does Not Exist");
            }

            var incidentResolution = db.GetDbRowOrNull(request.IncidentResolution.ToEntityReference());

            if (incidentResolution != null)
            {
                throw new FaultException("Cannot insert duplicate key.");
            }

            return new CloseIncidentResponse();
        }
    }
}
#endif