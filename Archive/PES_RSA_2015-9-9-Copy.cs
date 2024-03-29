﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace EAWS.Core.SilverBullet
{
    public static class PES_RSA
    {

        //to lookup the correct atribute to get the id from for matching base item to upia stereotype - cant set the type to be the base type or nothing will map to DM2 cleanly.
        //UPIA Stereotype, Property to use to get id referring back to the UML item, DM2 type name, dm2type
        static String[][] UPIA_Element_Id_Prop = new string[][] {
                            //new string [] {"EnterpriseModel", "base_Package"},  //NOt used yet
                            //new string [] {"View", "base_Package"}, //not used yet
                            new string [] {"ArchitectureDescription", "base_Package", "ArchitecturalDescription", "IndividualType"},
                            //new string [] {"Viewpoint", "base_Class"}, //not used yet
                            //new string [] {"Conform", "base_Dependency"}, //NOt used yet
                            new string [] {"Capability", "base_Class", "Capability", "IndividualType"},
                            new string [] {"CapabilityUsage", "base_UseCase", "Activity", "IndividualType"},
                            new string [] {"CapabilityRole", "base_Class", "Performer", "IndividualType"},
                            new string [] {"OperationalNode", "base_Class", "Performer", "IndividualType"},
                            new string [] {"OperationalTask", "base_Operation", "Activity", "IndividualType"},
                            new string [] {"OperationalNodeSpecification", "base_Interface", "Performer", "IndividualType"},
                            //new string [] {"CapabilityRealization", "base_Class", "Activity", "IndividualType"},  UPIA Spec specifically says this is ignored.
                            new string [] {"Information", "base_Class", "DataType", "IndividualType"},
                            new string [] {"InformationElement", "base_Class", "Data", "IndividualType"},
                            new string [] {"DataElement", "base_Class", "Data", "IndividualType"},  
                            new string [] {"Project", "base_Class", "ProjectType", "IndividualType"},
                            new string [] {"Milestone", "base_Class", "TemporalMeasureType", "IndividualType"},//formerly PeriodType
                            new string [] {"TemporalElement", "base_Class", "TemporalMeasure", "IndividualType"},//Temportal Measure may need more than one thing here!
                            new string [] {"DataExchange", "consumingTask", "Activity", "IndividualType"},    //for data exchange - need to add the contained producing and consuming activity as a UPIA Task since only appear here.
                            new string [] {"DataExchange", "producingTask", "Activity", "IndividualType"},    //same as above comment
                            
                            new string [] {"System", "base_Class", "System", "IndividualType"},
                            new string [] {"CommunicationSystem", "base_Class", "System", "IndividualType"},
                            new string [] {"Network", "base_Class", "System", "IndividualType"},
                            new string [] {"SystemHardware", "base_Class", "System", "IndividualType"},
                            new string [] {"SystemSoftware", "base_Class", "System", "IndividualType"},
                            new string [] {"MeasureType", "base_Class", "MeasureType", "IndividualType"},
                            new string [] {"ActualMeasure", "base_InstanceSpecification", "Measure", "IndividualType"},
                            new string [] {"MeasuredElement", "base_NamedElement", "HappensInType", "IndividualType"},
                            new string [] {"ServiceParticipant", "base_Class", "Service", "IndividualType"},  //is mentioned as just participant in the UPIA doc, but stores as serviceparticipant
                            new string [] {"ServiceSpecification", "base_Interface", "Service", "IndividualType"},
                            new string [] {"ProjectInstance",  "base_InstanceSpecification", "Project", "IndividualType"},
                            new string [] {"SystemTask",  "base_Operation", "Activity", "IndividualType"},
                            new string [] {"SystemFunction",  "base_Activity", "Activity", "IndividualType"},

                            new string [] {"Personnel",  "base_Class", "PersonType", "IndividualType"},
                            new string [] {"Stakeholder",  "base_Class", "PersonType", "IndividualType"},
                            new string [] {"OrganizationalResource",  "base_Class", "OrganizationType", "IndividualType"},
                            };

        //In order - DM2 Element Name, Stereotype in UPIA, DM2 type - Currently NOT USED
        static string[][] Element_Lookup = new string[][] { 
                            new string [] {"ArchitecturalDescription", "ArchitectureDescription", "IndividualType"},
                            new string [] {"Capability", "Capability", "IndividualType"},
                            //new string [] {"TemporalMeasure", "TemporalElement", "IndividualType"}, /actually a relationship (dependency)
                            //new string [] {"wholepart", "PartOf", "base_Association"},
                            new string [] {"Activity", "CapabilityUsage", "IndividualType"},
                            //new string [] {"activitypartofCapability", "ExercisesCapability", "base_Association"},  
                            new string [] {"Performer", "CapabilityRole", "IndividualType"},
                            new string [] {"Activity", "OperationalTask", "IndividualType"},
                            new string [] {"Performer", "OperationalNodeSpecification", "IndividualType"},
                            new string [] {"Activity", "CapabilityRealization", "IndividualType"},
                            new string [] {"Data", "Information", "IndividualType"},
                            new string [] {"DataType", "InformationElement", "IndividualType"},
                            new string [] {"Data", "DataElement", "IndividualType"},
                            new string [] {"ProjectType", "Project", "IndividualType"},
                            new string [] {"PeriodType", "Milestone", "IndividualType"},
                            //new string [] {"", "MilestonePoint", "base_Association"},
                            //new string [] {"", "ProjectStructure", "base_Association"},
                            //new string [] {"", "ProjectPhase", "base_Association"},
                            new string [] {"System", "System", "IndividualType"},
                            new string [] {"System", "CommunicationSystem", "IndividualType"},
                            new string [] {"MeasureType", "MeasureType", "IndividualType"},
                            new string [] {"Measure ", "ActualMeasure", "IndividualType"},
                            //new string [] {"", "DataExchange", "base_InformationFlow"},  special processing
                            //new string [] {"HappensInType", "MeasuredElement", "IndividualType"}, specialprocessing
                            new string [] {"Service", "ServiceParticipant", "IndividualType"},
                            new string [] {"Service", "ServiceSpecification", "IndividualType"},
                            new string [] {"Project", "ProjectInstance",  "IndividualType"},

                            //new string [] {"OperationalNodeSpecification", ""}  //relationship according to UPIA live data - interface realizaton... Couple or Tuple...  capabilityrole to 

                            /* From Previous SA work - kept single entries to ensure all types eventually accounted for
                            new string[] { "Activity", "Activity (DM2)", "IndividualType", "1154", "1326", "default" }, 
                            new string[] { "Capability", "Capability (DM2)", "IndividualType", "1155", "1327", "default" },
                            new string[] { "Performer", "Performer (DM2)", "IndividualType", "1178", "1367", "default" },
                            new string[] { "Activity", "System Function (DM2x)", "IndividualType", "1207", "1384", "extra" },
                            new string[] { "Service", "Service (DM2)", "IndividualType","1160", "1376", "default" },
                            new string[] { "Resource", "Resource (DM2)", "IndividualType","", "", "default" },
                            new string[] { "System", "System (DM2)", "IndividualType","1188", "1377", "default" },
                            new string[] { "Materiel", "Materiel (DM2)", "IndividualType","1177", "1366", "default" },
                            new string[] { "Information", "Information (DM2)", "IndividualType","1176", "1365", "default" },
                            new string[] { "PersonRole", "Person (DM2)", "IndividualType","1186", "1375", "default" },
                            new string[] { "DomainInformation", "DomainInformation (DM2)", "IndividualType","", "1371", "default" },
                            new string[] { "Data", "Table", "IndividualType","", "1370", "extra" },
                            new string[] { "OrganizationType", "Organization (DM2)", "IndividualType","1185", "1374", "default" },
                            new string[] { "Condition", "Condition (Environmental) (DM2)", "IndividualType","1156", "1328", "default" },
                            new string[] { "Location", "Location (DM2)", "IndividualType","1161", "", "default" },
                            new string[] { "RegionOfCountry", "RegionOfCountry (DM2)", "IndividualType","1357", "1357", "default" },
                            new string[] { "Country", "Country (DM2)", "IndividualType","1358", "1358", "default" },
                            new string[] { "Rule", "Rule (DM2)", "IndividualType","1173", "1362", "default" },
                            new string[] { "IndividualType", "IndividualType", "IndividualType","", "", "default" },
                            new string[] { "ArchitecturalDescription", "ArchitecturalDescription (DM2)", "IndividualType","1179", "1368","default" },
                            new string[] { "ServiceDescription", "ServiceDescription (DM2)", "IndividualType","", "1369","default" },
                            new string[] { "ProjectType", "Project (DM2)", "IndividualType","1159", "1348","default" },
                            new string[] { "Vision", "Vision (DM2)", "IndividualType","1172", "1361","default" },
                            new string[] { "Guidance", "Guidance (DM2)", "IndividualType","1157", "1329","default" },
                            new string[] { "Facility", "Facility (DM2)", "IndividualType","", "1353","default" },
                            new string[] { "RealProperty", "RealProperty (DM2)", "IndividualType","", "1352","default" },
                            new string[] { "Site", "Site (DM2)", "IndividualType","", "1354","default" },
                             */
                            };

        static string[][] SA_Element_View_Lookup = new string[][] { 
                            new string[] { "AV-01 Overview and Summary (DM2)", "RealProperty (DM2)", "Location (DM2)", "1161" },
                            new string[] { "AV-01 Overview and Summary (DM2)", "Facility (DM2)", "Location (DM2)", "1161" },
                            new string[] { "OV-02 Operational Resource Flow (DM2)", "Organization (DM2)", "Resource (DM2)", "1160" },
                            new string[] { "OV-02 Operational Resource Flow (DM2)", "Person (DM2)", "Performer (DM2)", "1178" },
        
                            }; 

        static string[][] RSA_Element_Lookup = new string[][] { 
                            new string[] { "Activity", "OperationalNodeSpecification", "IndividualType", "", "", "default" },
                            new string[] { "Capability", "Capability", "IndividualType", "", "", "default" },
                            new string[] { "System", "System", "IndividualType", "", "", "default" },

                            };
        
        static string[][] Tuple_Lookup = new string[][] { 
                            };
        
        //Done a bit differntly since SA has so many lines and UML really has few.  UPIA adds stereotypes on them that must be dealt with, similar to the regular things.
        //Not sure the UPIA mapping matters since there is no real translation of this now - nothing maped in the spec anyway. Nothing more deterministic discernablea at this point.
        static string[][] Tuple_Type_Lookup = new string[][] { 
                            //new string [] {"WholePartType (etc)", "DoDAF2TypeName", "UPIA StereoType", "Base ID to UML", "UML Type", SourceProp, DestProp}
                            //new string [] {"WholePartType", "WholePartType", "PartOf", "base_Association", "uml:Association", "memberEnd", ""},   //memberEnd comes in two parts separated by a space, IDs in both parts
                            //new string[] { "WholePartType", "WholePartType", "ExercisesCapability","uml:Association", "memberEnd", "navigableOwnedEnd" }, //memer end contains both sparated by space, navigableownedend is source
                            //new string[] { "WholePartType", "WholePartType", "MilestonePoint", "uml:Association", "memberEnd", ""},
                            //new string[] { "WholePartType", "WholePartType", "ProjectStructure", "uml:Association", "memberEnd", ""},
                            //new string[] { "WholePartType", "WholePartType", "ProjectPhase", "uml:Association", "memberEnd", ""},
                            //commenting out above. Not deterministic nor documented anywhere what UPIA types map to what DM2 types. May attempt again another time.
                            new string[] { "WholePartType", "WholePartType", "uml:Association", "uml:Association", "memberEnd", ""},  //for now letting this single entry deal with UPIA PartOf as well as normal association till I learn otherwise.  The live data does not always use the UPIA stereotype
                            new string[] { "WholePartType", "WholePartType", "uml:Dependency", "uml:Dependency", "supplier", "client"},
                            new string[] { "WholePartType", "WholePartType", "uml:Usage", "uml:Usage", "supplier", "client"},
                            new string[] { "WholePartType", "WholePartType", "uml:Realization", "uml:Realization", "supplier", "client"},
                            new string[] { "WholePartType", "WholePartType", "uml:InformationFlow", "uml:InformationFlow", "informationSource", "informationTarget"},
                            new string[] { "WholePartType", "activityPartOfCapability", "ExercisesCapability","uml:Association", "memberEnd", "navigableOwnedEnd" },
                            
/*
                            new string[] { "WholePartType", "Data Element", "WholePartType", "1", "Attribute", "Attribute" },
                            new string[] { "WholePartType", "Table Name", "WholePartType", "2", "Attribute", "Attribute" },
                            
                            new string[] { "WholePartType", "Foreign Keys and Roles", "WholePartType", "1", "Attribute", "Attribute" },
                            new string[] { "WholePartType", "Constraint Name", "WholePartType", "2", "Attribute", "Attribute" },

                            new string[] { "WholePartType", "Primary Index Name", "WholePartType", "1", "Element", "Element" },
                            //new string[] { "WholePartType", "Primary Key Name", "WholePartType", "2", "Element", "Element" },
                            new string[] { "WholePartType", "Entity Name", "WholePartType", "2", "Attribute", "Attribute" },
                            new string[] { "WholePartType", "activityParentOfActivity", "WholePartType", "1", "Activity (DM2)", "Activity" },
                            new string[] { "WholePartType", "activityPartOfActivity", "WholePartType", "2", "Activity (DM2)", "Activity" },
                            new string[] { "WholePartType", "Parent Of Capability", "WholePartType", "1", "Capability (DM2)", "Capability" },
                            new string[] { "WholePartType", "Parent Capability", "WholePartType", "2", "Capability (DM2)", "Capability" },
                            new string[] { "WholePartType", "Parent of Function", "WholePartType", "1", "Activity (DM2)", "Activity" },
                            new string[] { "WholePartType", "Part of Function", "WholePartType", "2", "Activity (DM2)", "Activity" },
                            new string[] { "WholePartType", "Parent Of Service", "WholePartType", "1", "Service (DM2)", "Service" },
                            new string[] { "WholePartType", "Part of Service", "WholePartType", "2", "Service (DM2)", "Service" },
                            new string[] { "WholePartType", "Parent of System", "WholePartType", "1", "System (DM2)", "System" }, 
                            new string[] { "WholePartType", "Part of System", "WholePartType", "2", "System (DM2)", "System" }, 
                            new string[] { "WholePartType", "PerformerMembers", "WholePartType", "1", "Organization (DM2)", "OrganizationType" },
                            new string[] { "WholePartType", "MemberOfOrganizations", "WholePartType", "2", "Organization (DM2)", "OrganizationType" },
                            new string[] { "WholePartType", "personPartOfPerformer", "WholePartType", "1", "Performer (DM2)", "Performer" },
                            new string[] { "WholePartType", "personPartOfPerformer", "WholePartType", "2", "Person (DM2)", "PersonRole" }, 
                            new string[] { "WholePartType", "portPartOfPerformer", "WholePartType", "5", "System (DM2)", "System" },
                            new string[] { "WholePartType", "portPartOfPerformer", "WholePartType", "4", "Interface (Port) (DM2)", "Performer" },
                            new string[] { "activityPartOfProjectType", "activityPartOfProjectType", "WholePartType", "5", "Project (DM2)", "ProjectType" },
                            new string[] { "activityPartOfProjectType", "activityPartOfProjectType", "WholePartType", "4", "Activity (DM2)", "Activity" },
                            new string[] { "WholePartType", "interfacePartOfService", "WholePartType", "5", "Service (DM2)", "Service" },
                            new string[] { "WholePartType", "interfacePartOfService", "WholePartType", "4", "ServiceInterface (DM2)", "Performer" },
                            //new string[] { "activityPartOfProjectType", "Milestones", "WholePartType", "1", "Activity (DM2)", "Activity" },
                            //new string[] { "activityPartOfProjectType", "Project", "WholePartType", "2", "Project (DM2)", "ProjectType" },
                            new string[] { "SupportedBy", "SupportedBy", "SupportedBy", "3", "Organization (DM2)", "OrganizationType" },
                            new string[] { "ruleConstrainsActivity", "ruleConstrainsActivity", "CoupleType", "2", "Activity (DM2)", "Activity" },
 */
                            };

        static string[][] SA_Element_Lookup = new string[][] {
                            new string[] { "Needline", "Need Line (DM2rx)", "CoupleType","1244", "1402","default" },
                            new string[] { "SysRF", "System Resource Flow (DM2rx)", "CoupleType","1245", "1403","default" },
                            new string[] { "PRF", "Physical Resource Flow (DM2rx)", "CoupleType","1475", "1462","default" },
                            new string[] { "SvcRF", "Service Resource Flow (DM2rx)", "CoupleType","1236", "1394","default" },
                            new string[] { "SDF", "System Data Flow (DM2rx)", "CoupleType","1215", "1386","default" },
                            new string[] { "SvcDF", "Service Data Flow (DM2rx)", "CoupleType","1239", "1396","default" },
                            new string[] { "CapabilityDependency", "Capability Dependency (DM2rx)", "CoupleType","1433", "1449","default" },
                            new string[] { "ARO", "ActivityResourceOverlap (DM2r)", "CoupleType","1208", "1383","default" },
                            };
        
        //a great deal of overlap of dodaf views into UML for RSA
        static string[][] View_Lookup = new string[][] {  
                            //Priority 1
                            new string[] {"CV-3", "Class", "", "default"},
                            new string[] {"CV-3", "Freeform", "", "extra"},
                            new string[] {"CV-6", "Class", "", "default"},
                            //new string[] {"CV-6", "Usecase", "", "extra"}, //not used afterall
                            new string[] {"DIV-1", "Class", "", "Default"},
                            new string[] {"DIV-2", "Class", "", "Default"},
                            new string[] {"PV-2", "Class", "", "Default"},
                            new string[] {"SV-6", "Freeform", "", "Default"},
                            new string[] {"SvcV-6", "Freeform", "", "Default"},
                            //Priority 2
                            new string[] {"CV-2", "Class", "", "default"},
                            new string[] {"CV-2", "Freeform", "", "extra"},
                            new string[] {"DIV-3", "Class", "", "Default"},
                            /*
                            //Later increments
                            //new string[] {"CV-1", "Class", "", "default"}, //not on SOW
                            //new string[] {"CV-1", "Freeform", "", "extra"}, //not on SOW
                            new string[] {"CV-2", "Class", "", "default"},
                            //new string[] {"CV-4", "Freeform", "", "default"}, //not on SOW
                            new string[] {"DIV-3", "Class", "", "default"},
                            new string[] {"DIV-3", "Freeform", "", "extra"},
                            new string[] {"OV-1", "Freeform", "", "default"},
                            new string[] {"OV-2", "Class", "", "default"},
                            new string[] {"OV-4", "Class", "", "default"},
                            new string[] {"OV-4", "Freeform", "", "extra"},
                            new string[] {"OV-5a", "Activity", "", "default"},
                            new string[] {"OV-5a", "Freeform", "", "extra"},
                            new string[] {"OV-5a", "Usecase", "", "extra"},
                            new string[] {"OV-5b", "Activity", "", "default"},
                            new string[] {"OV-5b", "Freeform", "", "extra"},
                            new string[] {"OV-5b", "Topic", "", "extra"},
                            new string[] {"OV-5b", "Usecase", "", "extra"},
                            new string[] {"OV-6a", "Class", "", "default"},
                            new string[] {"OV-6b", "Statechart", "", "default"},
                            new string[] {"OV-6c", "Sequence", "", "default"},
                            //new string[] {"PV-1", "??", "", "default"},  //NA
                            new string[] {"SV-1", "Freeform", "", "default"},
                            new string[] {"SV-1", "Topic", "", "extra"},
                            new string[] {"SV-2", "Freeform", "", "default"},
                            new string[] {"SV-2", "Class", "", "extra"},
                            new string[] {"SV-2", "Compositestructure", "", "default"},
                            new string[] {"SV-4", "Activity", "", "default"},
                            new string[] {"SV-4", "Usecase", "", "extra"},
                            new string[] {"SV-4", "Freeform", "", "extra"},
                            //new string[] {"SV-8", "", "", "default"},  //NA for RSA
                            //new string[] {"SV-10c", "??", "", "default"},
                            //new string[] {"SvcV-1", "??", "", "default"},   //TBD
                            //new string[] {"SvcV-2", "??", "", "default"},   //TBD
                            //new string[] {"SvcV-4", "??", "", "default"},   //TBD
                            //new string[] {"SvcV-5", "??", "", "default"},   //TBD
                            */
                            };
        //There are limited diagram types availabe in UML and they don't distinquish between the ways they are used, therfore can only put those that are completely ignored
        static string[][] Not_Processed_View_Lookup = new string[][] {  
                            new string[] {"Unmappedview1", "Component", "", "default"},
                            new string[] {"Unmappedview2", "Deployment", "", "default"},
                            new string[] {"Unmappedview3", "Communication", "", "default"},
                            new string[] {"Unmappedview4", "Object", "", "default"},
                            };

        static string[][] Mandatory_Lookup = new string[][] { 
                            //Priority 1 - CV3
                            new string[] {"Capability", "CV-3"},
                            new string[] {"Activity", "CV-3"},
                            new string[] {"ProjectType", "CV-3"},
                            new string[] {"activityPartOfCapability", "CV-3"},
                            new string[] {"activityPartOfProjectType", "CV-3"},
                            new string[] {"desiredResourceStateOfCapability", "CV-3"},
                            //Priority. 1 - CV-6
                            new string[] {"Capability", "CV-6"},
                            new string[] {"Activity", "CV-6"},
                            new string[] {"activityPartOfCapability", "CV-6"},
                            //Priority. 1 - PV-2
                            new string[] {"Activity", "PV-2"},
                            new string[] {"activityPartOfProjectType", "PV-2"},
                            new string[] {"ProjectType", "PV-2"},
                            //Priority. 1 - Div-1
                            //NO MANDATORY ELEMENTS FOR DIV 1
                            //Priority. 1 - Div-2
                            new string[] {"Data", "DIV-2"},
                            //Inc. 1 - SV-6
                            new string[] {"Activity", "SV-6"},
                            new string[] {"activityPerformedByPerformer", "SV-6"},
                            new string[] {"activityProducesResource", "SV-6"},
                            new string[] {"activityConsumesResource", "SV-6"},
                            new string[] {"System", "SV-6"},
                            new string[] {"Data", "SV-6"},
                            //Inc. 1 - SvcV-6
                            new string[] {"Activity", "SvcV-6"},
                            new string[] {"activityPerformedByPerformer", "SvcV-6"},
                            new string[] {"activityProducesResource", "SvcV-6"},
                            new string[] {"activityConsumesResource", "SvcV-6"},
                            new string[] {"Service", "SvcV-6"},
                            new string[] {"Data", "SvcV-6"},
                            new string[] {"ServiceDescription", "SvcV-6"},
                            new string[] {"serviceDescribedBy", "SvcV-6"},

                            //Priority 1 - CV-2
                            new string[] {"Capability", "CV-2"},
                            //Priority. 1 - Div-3
                            new string[] {"Data", "DIV-3"},
                            new string[] {"DataType", "DIV-3"},


                            //END REQUIREMENTS - COMMENTING OUT THE REST FOR NOW>
                            /*new string[] {"ArchitecturalDescription", "OV-1"},
                            new string[] {"Activity", "OV-5a"},
                            new string[] {"Activity", "OV-2"},
                            new string[] {"activityPerformedByPerformer", "OV-2"},
                            new string[] {"activityProducesResource", "OV-2"},
                            new string[] {"activityConsumesResource", "OV-2"},
                            new string[] {"Activity", "OV-3"},
                            new string[] {"activityPerformedByPerformer", "OV-3"},
                            new string[] {"activityProducesResource", "OV-3"},
                            new string[] {"activityConsumesResource", "OV-3"},
                            
                             * new string[] {"Activity", "OV-5b"},
                            new string[] {"activityProducesResource", "OV-5b"},
                            new string[] {"activityConsumesResource", "OV-5b"},
                            new string[] {"Activity", "OV-6b"},
                            new string[] {"activityProducesResource", "OV-6b"},
                            new string[] {"activityConsumesResource", "OV-6b"},
                            new string[] {"Activity", "OV-6a"},
                            new string[] {"Activity", "AV-1"},
                            new string[] {"activityPartOfProjectType", "AV-1"},
                            new string[] {"ArchitecturalDescription", "AV-1"},
                            new string[] {"ProjectType", "AV-1"},
                            new string[] {"Capability", "CV-1"},
                            new string[] {"desiredResourceStateOfCapability", "CV-1"},
                            new string[] {"desireMeasure", "CV-1"},
                            new string[] {"effectMeasure", "CV-1"},
                            new string[] {"MeasureOfDesire", "CV-1"},
                            new string[] {"MeasureOfEffect", "CV-1"},
                            new string[] {"visionRealizedByDesiredResourceState", "CV-1"},
                            new string[] {"Vision", "CV-1"},
                            new string[] {"Capability", "CV-4"},
                            new string[] {"desiredResourceStateOfCapability", "CV-4"},
                            new string[] {"Activity", "OV-6c"},
                            new string[] {"Activity", "SV-1"},
                            new string[] {"activityPerformedByPerformer", "SV-1"},
                            new string[] {"activityProducesResource", "SV-1"},
                            new string[] {"activityConsumesResource", "SV-1"},
                            new string[] {"System", "SV-1"},
                            new string[] {"Activity", "SV-10b"},
                            new string[] {"activityPerformedByPerformer", "SV-10b"},
                            new string[] {"activityProducesResource", "SV-10b"},
                            new string[] {"activityConsumesResource", "SV-10b"},
                            new string[] {"System", "SV-10b"},
                            new string[] {"Activity", "SV-10c"},
                            new string[] {"activityPerformedByPerformer", "SV-10c"},
                            new string[] {"activityProducesResource", "SV-10c"},
                            new string[] {"activityConsumesResource", "SV-10c"},
                            new string[] {"System", "SV-10c"},
                            new string[] {"Activity", "SvcV-1"},
                            new string[] {"activityPerformedByPerformer", "SvcV-1"},
                            new string[] {"activityProducesResource", "SvcV-1"},
                            new string[] {"activityConsumesResource", "SvcV-1"},
                            new string[] {"Service", "SvcV-1"},
                            new string[] {"Activity", "SvcV-10b"},
                            new string[] {"activityPerformedByPerformer", "SvcV-10b"},
                            new string[] {"activityProducesResource", "SvcV-10b"},
                            new string[] {"activityConsumesResource", "SvcV-10b"},
                            new string[] {"Service", "SvcV-10b"},
                            new string[] {"Activity", "SvcV-10c"},
                            new string[] {"activityPerformedByPerformer", "SvcV-10c"},
                            new string[] {"activityProducesResource", "SvcV-10c"},
                            new string[] {"activityConsumesResource", "SvcV-10c"},
                            new string[] {"Service", "SvcV-10c"},
                            new string[] {"Activity", "SV-4"},
                            new string[] {"activityPerformedByPerformer", "SV-4"},
                            new string[] {"activityProducesResource", "SV-4"},
                            new string[] {"activityConsumesResource", "SV-4"},
                            new string[] {"Data", "SV-4"},
                            new string[] {"System", "SV-4"},
                            new string[] {"Activity", "SvcV-4"},
                            new string[] {"activityPerformedByPerformer", "SvcV-4"},
                            new string[] {"activityProducesResource", "SvcV-4"},
                            new string[] {"activityConsumesResource", "SvcV-4"},
                            new string[] {"Data", "SvcV-4"},
                            new string[] {"Service", "SvcV-4"},
                            new string[] {"Activity", "SV-2"},
                            new string[] {"activityPerformedByPerformer", "SV-2"},
                            new string[] {"activityProducesResource", "SV-2"},
                            new string[] {"activityConsumesResource", "SV-2"},
                            new string[] {"System", "SV-2"},
                            new string[] {"System", "SV-8"},
                            new string[] {"Activity", "SvcV-2"},
                            new string[] {"activityPerformedByPerformer", "SvcV-2"},
                            new string[] {"activityProducesResource", "SvcV-2"},
                            new string[] {"activityConsumesResource", "SvcV-2"},
                            new string[] {"Service", "SvcV-2"},
                            new string[] {"Activity", "PV-1"},
                            new string[] {"activityPartOfProjectType", "PV-1"},
                            new string[] {"ProjectType", "PV-1"},
                            new string[] {"activityPerformedByPerformer", "PV-1"},
                            new string[] {"OrganizationType", "PV-1"} */
                            };

        static string[][] Optional_Lookup = new string[][] { 
                            //SOW does not require optional elements but trying
                            //Inc. 1- CV-3
                            //Optionals are too numerous on monster matrix  Will do only those required to reproduce live data.
                            new string[] {"WholePartType", "CV-3"}, 

                            //Inc. 1 - CV-6
                            new string[] {"TemporalMeasure", "CV-6"},
                            new string[] {"TemporalMeasureType", "CV-6"},
                            new string[] {"WholePartType", "CV-6"}, 
                            //Inc. 1 - PV-2
                            new string[] {"PeriodType", "PV-2"},
                            new string[] {"HappensInType", "PV-2"},
                            new string[] {"Condition", "PV-2"},
                            new string[] {"Information", "PV-2"},
                            new string[] {"Location", "PV-2"},
                            new string[] {"Performer", "PV-2"},
                            new string[] {"Resource", "PV-2"},
                            new string[] {"Rule", "PV-2"}, 
                            new string[] {"superSubtype", "PV-2"}, 
                            new string[] {"WholePartType", "PV-2"}, 
                            new string[] {"activityPerformedByPerformer", "PV-2"},
                            new string[] {"Project", "PV-2"},
                            new string[] {"TemporalMeasureType", "PV-2"},
                            new string[] {"TemporalMeasure", "PV-2"},

                            //Inc. 1 - Div-1
                            new string[] {"Data","DIV-1"},
                            new string[] {"DataType", "DIV-1"},
                            new string[] {"WholePartType", "DIV-1"},  //Not strictly required at all - but no way to reproduce the live data without it.
                            //Inc. 1 - Div-2
                            new string[] {"DataType", "DIV-2"},
                            new string[] {"Location", "DIV-2"},
                            new string[] {"OrganizationType", "DIV-2"},
                            new string[] {"Performer", "DIV-2"},
                            new string[] {"Resource", "DIV-2"},
                            new string[] {"Rule", "DIV-2"}, 
                            new string[] {"superSubtype", "DIV-2"}, 
                            new string[] {"WholePartType", "DIV-2"},
                            new string[] {"OverlapType", "DIV-2"},
                            //Inc. 1 - SV-6
                            new string[] {"Measure", "SV-6"},
                            new string[] {"MeasureType", "SV-6"},
                            new string[] {"WholePartType", "SV-6"}, 
                            //Not dealing in all optionals for SV-6 since not required and only one partial one exists in live data
                            //Inc. 1 - SvcV-6 - not dealing in optionals for SvcV-6 since not required and NONE exist in the live data
                            //Priority 2 - CV-2
                            new string[] {"Activity", "CV-2"},
                            new string[] {"Condition", "CV-2"},
                            new string[] {"DomainInformation", "CV-2"},
                            new string[] {"Information", "CV-2"},
                            new string[] {"Location", "CV-2"},
                            new string[] {"Performer", "CV-2"},
                            new string[] {"PersonRole", "CV-2"},
                            new string[] {"Resource", "CV-2"},
                            new string[] {"Rule", "CV-2"}, 
                            new string[] {"System", "CV-2"},
                            new string[] {"Service", "CV-2"},
                            new string[] {"superSubtype", "CV-2"}, 
                            new string[] {"WholePartType", "CV-2"}, 
                            new string[] {"BeforeAfterType", "CV-2"},

                            //Priority2 - DIV-3
                            new string[] {"Condition", "DIV-3"},
                            new string[] {"Information", "DIV-3"},
                            new string[] {"Location", "DIV-3"},
                            new string[] {"OrganizationType", "DIV-3"},
                            new string[] {"Performer", "DIV-3"},
                            new string[] {"Resource", "DIV-3"},
                            new string[] {"Rule", "DIV-3"}, 
                            new string[] {"superSubtype", "DIV-3"}, 
                            new string[] {"WholePartType", "DIV-3"},
                            new string[] {"typeInstance", "DIV-3"},

                            //COMMENTING OUT THE REST FOR NOW>
                            /*new string[] {"Activity", "CV-1"},
                            new string[] {"Condition", "CV-1"},
                            new string[] {"DomainInformation", "CV-1"},
                            new string[] {"Information", "CV-1"},
                            new string[] {"Location", "CV-1"},
                            new string[] {"Performer", "CV-1"},
                            new string[] {"PersonRole", "CV-1"},
                            new string[] {"Resource", "CV-1"},
                            new string[] {"Rule", "CV-1"}, 
                            new string[] {"System", "CV-1"},
                            new string[] {"Service", "CV-1"},
                            new string[] {"ServiceDescription", "CV-1"},
                            new string[] {"superSubtype", "CV-1"}, 
                            new string[] {"WholePartType", "CV-1"},
                            new string[] {"activityPartOfCapability", "CV-1"}, 
                            new string[] {"Activity", "CV-4"},
                            new string[] {"activityPerformedByPerformer", "CV-4"},
                            new string[] {"activityProducesResource", "CV-4"},
                            new string[] {"activityConsumesResource", "CV-4"},
                            new string[] {"BeforeAfterType", "CV-4"},
                            new string[] {"Condition", "CV-4"},
                            new string[] {"DomainInformation", "CV-4"},
                            new string[] {"Information", "CV-4"},
                            new string[] {"Location", "CV-4"},
                            new string[] {"Performer", "CV-4"},
                            new string[] {"PersonRole", "CV-4"},
                            new string[] {"OrganizationType", "CV-4"},
                            new string[] {"Resource", "CV-4"},
                            new string[] {"Rule", "CV-4"}, 
                            new string[] {"System", "CV-4"},
                            new string[] {"Service", "CV-4"},
                            new string[] {"ServiceDescription", "CV-4"},
                            new string[] {"superSubtype", "CV-4"}, 
                            new string[] {"WholePartType", "CV-4"},
                            new string[] {"activityPartOfCapability", "CV-4"}, 
                            new string[] {"Information", "OV-1"},
                            new string[] {"Location", "OV-1"},
                            new string[] {"Performer", "OV-1"},
                            new string[] {"Resource", "OV-1"},
                            new string[] {"Rule", "OV-1"}, 
                            new string[] {"superSubtype", "OV-1"}, 
                            new string[] {"WholePartType", "OV-1"},
                            new string[] {"representationSchemeInstance", "OV-1"},
                            new string[] {"Condition", "OV-2"},
                            new string[] {"Information", "OV-2"},
                            new string[] {"Location", "OV-2"},
                            new string[] {"OrganizationType", "OV-2"},
                            new string[] {"Performer", "OV-2"},
                            new string[] {"PersonRole", "OV-2"},
                            new string[] {"Resource", "OV-2"},
                            new string[] {"Rule", "OV-2"}, 
                            new string[] {"superSubtype", "OV-2"}, 
                            new string[] {"WholePartType", "OV-2"},
                            new string[] {"Condition", "OV-3"},
                            new string[] {"Information", "OV-3"},
                            new string[] {"Location", "OV-3"},
                            new string[] {"OrganizationType", "OV-3"},
                            new string[] {"Performer", "OV-3"},
                            new string[] {"PersonRole", "OV-3"},
                            new string[] {"Resource", "OV-3"},
                            new string[] {"Rule", "OV-3"}, 
                            new string[] {"superSubtype", "OV-3"}, 
                            new string[] {"WholePartType", "OV-3"},
                            new string[] {"Condition", "SV-6"},
                            new string[] {"Information", "SV-6"},
                            new string[] {"Location", "SV-6"},
                            new string[] {"OrganizationType", "SV-6"},
                            new string[] {"Performer", "SV-6"},
                            new string[] {"PersonRole", "SV-6"},
                            new string[] {"Resource", "SV-6"},
                            new string[] {"Rule", "SV-6"}, 
                            new string[] {"superSubtype", "SV-6"}, 
                            new string[] {"WholePartType", "SV-6"},
                            new string[] {"Information", "OV-4"},
                            new string[] {"Location", "OV-4"},
                            new string[] {"OrganizationType", "OV-4"},
                            new string[] {"Performer", "OV-4"},
                            new string[] {"PersonRole", "OV-4"},
                            new string[] {"Resource", "OV-4"},
                            new string[] {"Rule", "OV-4"}, 
                            new string[] {"superSubtype", "OV-4"}, 
                            new string[] {"WholePartType", "OV-4"},
                            new string[] {"Condition", "OV-5a"},
                            new string[] {"Information", "OV-5a"},
                            new string[] {"Location", "OV-5a"},
                            new string[] {"Performer", "OV-5a"},
                            new string[] {"Resource", "OV-5a"},
                            new string[] {"Rule", "OV-5a"}, 
                            new string[] {"superSubtype", "OV-5a"}, 
                            new string[] {"WholePartType", "OV-5a"}, 
                            new string[] {"Condition", "OV-5b"},
                            new string[] {"Information", "OV-5b"},
                            new string[] {"Location", "OV-5b"},
                            new string[] {"OrganizationType", "OV-5b"},
                            new string[] {"Performer", "OV-5b"},
                            new string[] {"PersonRole", "OV-5b"},
                            new string[] {"Resource", "OV-5b"},
                            new string[] {"Rule", "OV-5b"}, 
                            new string[] {"superSubtype", "OV-5b"}, 
                            new string[] {"WholePartType", "OV-5b"},
                            new string[] {"activityPerformedByPerformer", "OV-5b"},
                            new string[] {"Condition", "OV-6b"},
                            new string[] {"Information", "OV-6b"},
                            new string[] {"Location", "OV-6b"},
                            new string[] {"OrganizationType", "OV-6b"},
                            new string[] {"Performer", "OV-6b"},
                            new string[] {"PersonRole", "OV-6b"},
                            new string[] {"Resource", "OV-6b"},
                            new string[] {"Rule", "OV-6b"}, 
                            new string[] {"superSubtype", "OV-6b"}, 
                            new string[] {"WholePartType", "OV-6b"},
                            new string[] {"activityPerformedByPerformer", "OV-6b"},
                            new string[] {"BeforeAfterType", "OV-6b"},
                            new string[] {"Condition", "OV-6c"},
                            new string[] {"Information", "OV-6c"},
                            new string[] {"Location", "OV-6c"},
                            new string[] {"OrganizationType", "OV-6c"},
                            new string[] {"Performer", "OV-6c"},
                            new string[] {"PersonRole", "OV-6c"},
                            new string[] {"Resource", "OV-6c"},
                            new string[] {"Rule", "OV-6c"}, 
                            new string[] {"superSubtype", "OV-6c"}, 
                            new string[] {"WholePartType", "OV-6c"},
                            new string[] {"activityPerformedByPerformer", "OV-6c"},
                            new string[] {"Condition", "OV-6a"},
                            new string[] {"Information", "OV-6a"},
                            new string[] {"Location", "OV-6a"},
                            new string[] {"OrganizationType", "OV-6a"},
                            new string[] {"Performer", "OV-6a"},
                            new string[] {"PersonRole", "OV-6a"},
                            new string[] {"Resource", "OV-6a"},
                            new string[] {"Rule", "OV-6a"}, 
                            new string[] {"superSubtype", "OV-6a"}, 
                            new string[] {"WholePartType", "OV-6a"},
                            new string[] {"ruleConstrainsActivity", "OV-6a"},
                            new string[] {"activityPerformedByPerformer", "OV-6a"},
                            new string[] {"Condition", "AV-1"},
                            new string[] {"Facility", "AV-1"},
                            new string[] {"Guidance", "AV-1"},
                            new string[] {"Information", "AV-1"},
                            new string[] {"Location", "AV-1"},
                            new string[] {"OrganizationType", "AV-1"},
                            new string[] {"Performer", "AV-1"},
                            new string[] {"RealProperty", "AV-1"}, 
                            new string[] {"Resource", "AV-1"},
                            new string[] {"Rule", "AV-1"}, 
                            new string[] {"Site", "AV-1"}, 
                            new string[] {"Vision", "AV-1"},
                            new string[] {"superSubtype", "AV-1"}, 
                            new string[] {"WholePartType", "AV-1"}, 
                            new string[] {"ruleConstrainsActivity", "AV-1"}, 
                            new string[] {"Condition", "SV-1"},
                            new string[] {"Information", "SV-1"},
                            new string[] {"Location", "SV-1"},
                            new string[] {"OrganizationType", "SV-1"},
                            new string[] {"Performer", "SV-1"},
                            new string[] {"PersonRole", "SV-1"},
                            new string[] {"Resource", "SV-1"},
                            new string[] {"Rule", "SV-1"}, 
                            new string[] {"superSubtype", "SV-1"}, 
                            new string[] {"WholePartType", "SV-1"},
                            new string[] {"Condition", "SV-10b"},
                            new string[] {"Information", "SV-10b"},
                            new string[] {"Location", "SV-10b"},
                            new string[] {"OrganizationType", "SV-10b"},
                            new string[] {"Performer", "SV-10b"},
                            new string[] {"PersonRole", "SV-10b"},
                            new string[] {"Resource", "SV-10b"},
                            new string[] {"Rule", "SV-10b"}, 
                            new string[] {"superSubtype", "SV-10b"}, 
                            new string[] {"WholePartType", "SV-10b"},
                            new string[] {"BeforeAfterType", "SV-10b"},
                            new string[] {"Condition", "SV-10c"},
                            new string[] {"Information", "SV-10c"},
                            new string[] {"Location", "SV-10c"},
                            new string[] {"OrganizationType", "SV-10c"},
                            new string[] {"Performer", "SV-10c"},
                            new string[] {"PersonRole", "SV-10c"},
                            new string[] {"Resource", "SV-10c"},
                            new string[] {"Rule", "SV-10c"}, 
                            new string[] {"superSubtype", "SV-10c"}, 
                            new string[] {"WholePartType", "SV-10c"},
                            new string[] {"Condition", "SV-2"},
                            new string[] {"Information", "SV-2"},
                            new string[] {"Location", "SV-2"},
                            new string[] {"OrganizationType", "SV-2"},
                            new string[] {"Performer", "SV-2"},
                            new string[] {"PersonRole", "SV-2"},
                            new string[] {"Resource", "SV-2"},
                            new string[] {"Rule", "SV-2"}, 
                            new string[] {"superSubtype", "SV-2"}, 
                            new string[] {"WholePartType", "SV-2"},
                            new string[] {"Condition", "SvcV-1"},
                            new string[] {"Information", "SvcV-1"},
                            new string[] {"Location", "SvcV-1"},
                            new string[] {"OrganizationType", "SvcV-1"},
                            new string[] {"Performer", "SvcV-1"},
                            new string[] {"PersonRole", "SvcV-1"},
                            new string[] {"Resource", "SvcV-1"},
                            new string[] {"Rule", "SvcV-1"}, 
                            new string[] {"System", "SvcV-1"}, 
                            new string[] {"superSubtype", "SvcV-1"}, 
                            new string[] {"WholePartType", "SvcV-1"},
                            new string[] {"Condition", "SvcV-10b"},
                            new string[] {"Information", "SvcV-10b"},
                            new string[] {"Location", "SvcV-10b"},
                            new string[] {"OrganizationType", "SvcV-10b"},
                            new string[] {"Performer", "SvcV-10b"},
                            new string[] {"PersonRole", "SvcV-10b"},
                            new string[] {"Resource", "SvcV-10b"},
                            new string[] {"Rule", "SvcV-10b"}, 
                            new string[] {"System", "SvcV-10b"}, 
                            new string[] {"superSubtype", "SvcV-10b"}, 
                            new string[] {"WholePartType", "SvcV-10b"},
                            new string[] {"BeforeAfterType", "SvcV-10b"},
                            new string[] {"Condition", "SvcV-10c"},
                            new string[] {"Information", "SvcV-10c"},
                            new string[] {"Location", "SvcV-10c"},
                            new string[] {"OrganizationType", "SvcV-10c"},
                            new string[] {"Performer", "SvcV-10c"},
                            new string[] {"PersonRole", "SvcV-10c"},
                            new string[] {"Resource", "SvcV-10c"},
                            new string[] {"Rule", "SvcV-10c"}, 
                            new string[] {"System", "SvcV-10c"}, 
                            new string[] {"superSubtype", "SvcV-10c"}, 
                            new string[] {"WholePartType", "SvcV-10c"},
                            new string[] {"Condition", "SV-4"},
                            new string[] {"Information", "SV-4"},
                            new string[] {"Location", "SV-4"},
                            new string[] {"OrganizationType", "SV-4"},
                            new string[] {"Performer", "SV-4"},
                            new string[] {"PersonRole", "SV-4"},
                            new string[] {"Resource", "SV-4"},
                            new string[] {"Rule", "SV-4"}, 
                            new string[] {"superSubtype", "SV-4"}, 
                            new string[] {"WholePartType", "SV-4"},
                            new string[] {"Activity", "SV-8"},
                            new string[] {"Condition", "SV-8"},
                            new string[] {"Information", "SV-8"},
                            new string[] {"Location", "SV-8"},
                            new string[] {"OrganizationType", "SV-8"},
                            new string[] {"Performer", "SV-8"},
                            new string[] {"PersonRole", "SV-8"},
                            new string[] {"Resource", "SV-8"},
                            new string[] {"Rule", "SV-8"}, 
                            new string[] {"superSubtype", "SV-8"}, 
                            new string[] {"WholePartType", "SV-8"},
                            new string[] {"BeforeAfterType", "SV-8"},
                            new string[] {"activityPerformedByPerformer", "SV-8"},
                            new string[] {"HappensInType", "SV-8"},
                            new string[] {"PeriodType", "SV-8"},
                            new string[] {"Condition", "SvcV-4"},
                            new string[] {"Information", "SvcV-4"},
                            new string[] {"Location", "SvcV-4"},
                            new string[] {"OrganizationType", "SvcV-4"},
                            new string[] {"Performer", "SvcV-4"},
                            new string[] {"PersonRole", "SvcV-4"},
                            new string[] {"Resource", "SvcV-4"},
                            new string[] {"Rule", "SvcV-4"}, 
                            new string[] {"System", "SvcV-4"},  
                            new string[] {"superSubtype", "SvcV-4"}, 
                            new string[] {"WholePartType", "SvcV-4"},
                            new string[] {"Condition", "SvcV-2"},
                            new string[] {"Information", "SvcV-2"},
                            new string[] {"Location", "SvcV-2"},
                            new string[] {"OrganizationType", "SvcV-2"},
                            new string[] {"Performer", "SvcV-2"},
                            new string[] {"PersonRole", "SvcV-2"},
                            new string[] {"Resource", "SvcV-2"},
                            new string[] {"Rule", "SvcV-2"}, 
                            new string[] {"System", "SvcV-2"}, 
                            new string[] {"superSubtype", "SvcV-2"}, 
                            new string[] {"WholePartType", "SvcV-2"},

                            new string[] {"Condition", "PV-1"},
                            new string[] {"Information", "PV-1"},
                            new string[] {"Location", "PV-1"},
                            new string[] {"Performer", "PV-1"},
                            new string[] {"Resource", "PV-1"},
                            new string[] {"Rule", "PV-1"}, 
                            new string[] {"superSubtype", "PV-1"}, 
                            new string[] {"WholePartType", "PV-1"}, 
                            new string[] {"Activity", "AV-2"},
                            new string[] {"ArchitecturalDescription", "AV-2"},
                            new string[] {"Capability", "AV-2"},
                            new string[] {"Condition", "AV-2"},
                            new string[] {"Data", "AV-2"},
                            new string[] {"Facility", "AV-2"},
                            new string[] {"Guidance", "AV-2"},
                            new string[] {"Information", "AV-2"},
                            new string[] {"Location", "AV-2"},
                            new string[] {"MeasureOfDesire", "AV-2"},
                            new string[] {"MeasureOfEffect", "AV-2"},
                            new string[] {"OrganizationType", "AV-2"},
                            new string[] {"Performer", "AV-2"},
                            new string[] {"PersonRole", "AV-2"},
                            new string[] {"ProjectType", "AV-2"},
                            new string[] {"RealProperty", "AV-2"}, 
                            new string[] {"Resource", "AV-2"},
                            new string[] {"Rule", "AV-2"}, 
                            new string[] {"Service", "AV-2"}, 
                            new string[] {"ServiceDescription", "AV-2"}, 
                            new string[] {"System", "AV-2"}, 
                            new string[] {"Site", "AV-2"}, 
                            new string[] {"Thing", "AV-2"}, 
                            new string[] {"Vision", "AV-2"},
                            new string[] {"superSubtype", "AV-2"}, 
                            new string[] {"WholePartType", "AV-2"}, 
                            new string[] {"ruleConstrainsActivity", "AV-2"}, 
                            new string[] {"Country", "AV-2"}, 
                            new string[] {"RegionOfCountry", "AV-2"}, 
                            new string[] {"PeriodType", "AV-2"}, 
                            new string[] {"DataType", "AV-2"}, */
                            };

        private class Thing
        {
            public string type;
            public string id; 
            public string name;
            public object value; 
            public string place1;
            public string place2;
            public string foundation;
            public string value_type;
        }

        private class Location
        {
            public string id;
            public string element_id; 
            public string top_left_x;
            public string top_left_y;
            public string top_left_z;
            public string bottom_right_x;
            public string bottom_right_y;
            public string bottom_right_z;
        }

        private class View
        {
            public string type;
            public string id;
            public string name;
            public List<Thing> mandatory;
            public List<Thing> optional;
        }

        private static string Resource_Flow_Type(string type, string view, string place1, string place2, Dictionary<string, Thing> things)
        {
            string type1 = things[place1].type;
            string type2 = things[place2].type;

            if (type == "SF" && view.Contains("SV"))
            {
                return "System Data Flow (DM2rx)";
            }

            if (type == "SF" && view.Contains("SvcV"))
            {
                if (type1 == "Service" && type2 == "Service")
                {
                    return "Service Resource Flow (DM2rx)";
                }
                else
                {
                    return "Service Data Flow (DM2rx)";
                }
            }
                
            if (type == "Needline" && view.Contains("SvcV"))
                return "Physical Resource Flow (DM2rx)";

            if (type == "Needline" && view.Contains("SV"))
            {
                if (type1 == "System" && type2 == "System")    
                    return "System Resource Flow (DM2rx)";
                else
                    return "Physical Resource Flow (DM2rx)";
            }
            else
                return "Need Line (DM2rx)";
                
        }

        public static void Decode(string base64String, string outputFileName)
        {
            byte[] binaryData;
            try
            {
                binaryData =
                   System.Convert.FromBase64String(base64String);
            }
            catch (System.ArgumentNullException)
            {
                System.Console.WriteLine("Base 64 string is null.");
                return;
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine("Base 64 string length is not " +
                   "4 or is not an even multiple of 4.");
                return;
            }

            // Write out the decoded data.
            System.IO.FileStream outFile;
            try
            {
                outFile = new System.IO.FileStream(outputFileName,
                                           System.IO.FileMode.Create,
                                           System.IO.FileAccess.Write);
                outFile.Write(binaryData, 0, binaryData.Length);
                outFile.Close();
            }
            catch (System.Exception exp)
            {
                // Error creating stream or writing to it.
                System.Console.WriteLine("{0}", exp.Message);
            }
        }

        public static string Encode(string inputFileName)
        {
            System.IO.FileStream inFile;
            byte[] binaryData;

            try
            {
                inFile = new System.IO.FileStream(inputFileName,
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read);
                binaryData = new Byte[inFile.Length];
                long bytesRead = inFile.Read(binaryData, 0,
                                     (int)inFile.Length);
                inFile.Close();
            }
            catch (System.Exception exp)
            {
                // Error creating stream or reading from it.
                System.Console.WriteLine("{0}", exp.Message);
                return null;
            }

            // Convert the binary input into Base64 UUEncoded output. 
            string base64String;
            try
            {
                base64String =
                  System.Convert.ToBase64String(binaryData,
                                         0,
                                         binaryData.Length);
            }
            catch (System.ArgumentNullException)
            {
                System.Console.WriteLine("Binary data array is null.");
                return null;
            }

            return base64String;
        }

        public static void MergeDictionaries<OBJ1, OBJ2>(this IDictionary<OBJ1, List<OBJ2>> dict1, IDictionary<OBJ1, List<OBJ2>> dict2)
        {
            foreach (var kvp2 in dict2)
            {
                if (dict1.ContainsKey(kvp2.Key))
                {
                    dict1[kvp2.Key].AddRange(kvp2.Value);
                    continue;
                }
                dict1.Add(kvp2);
            }
        }

        public static void MergeDictionaries<OBJ1, OBJ2>(this IDictionary<OBJ1, OBJ2> dict1, IDictionary<OBJ1, OBJ2> dict2)
        {
            foreach (KeyValuePair<OBJ1, OBJ2> entry in dict2)
            {
                dict1[entry.Key] = entry.Value;   
            }
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        private static string Find_DM2_Type (string input) {
        
            foreach(string[] current_lookup in Element_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[0];
            }
            return null;
        }

        private static string Find_DM2_Type_RSA(string input)
        {

            foreach (string[] current_lookup in RSA_Element_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[0];
            }
            return null;
        }

        private static string Find_Def_DM2_Type(string input, List<Thing> things)
        {
            foreach (Thing thing in things)
            {
                if (input == thing.id)
                    return thing.type;
            }
            return null;
        }

        private static string Find_DM2_View(string input)
        {

            foreach (string[] current_lookup in View_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[0];
            }
            return null;
        }
        
        private static string Find_View_SA_Minor_Type(string input)
        {

            foreach (string[] current_lookup in View_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[2];
            }
            return null;
        }

        private static string Find_Symbol_Element_SA_Minor_Type(ref string input, string view)
        {

            foreach (string[] first_lookup in Element_Lookup)
            {
                if (input == first_lookup[1])
                {
                    foreach (string[] second_lookup in SA_Element_View_Lookup)
                    {
                        if (view == second_lookup[0] && input == second_lookup[1])
                        {
                            input = second_lookup[2];
                            return second_lookup[3];
                        }
                    }
                    return first_lookup[3];
                }
                    
            }
            foreach (string[] current_lookup in SA_Element_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[3];
            }
            return null;
        }

        private static string Find_Definition_Element_SA_Minor_Type(string input)
        {

            foreach (string[] current_lookup in Element_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[4];
            }
            foreach (string[] current_lookup in SA_Element_Lookup)
            {
                if (input == current_lookup[1])
                    return current_lookup[4];
            }
            return null;
        }

        private static string Find_SA_Relationship_Type(string rela_type, string thing_type, string place)
        {

            foreach (string[] current_lookup in Tuple_Lookup)
            {
                if (rela_type == current_lookup[0] && thing_type == current_lookup[4] && (place == "1" ? (current_lookup[3] == "1" || current_lookup[3] == "5") : (current_lookup[3] == "2" || current_lookup[3] == "4")))
                        return current_lookup[1];
            }
            foreach (string[] current_lookup in Tuple_Type_Lookup)
            {
                if (rela_type == current_lookup[0] && thing_type == current_lookup[4] && (place == "1" ? (current_lookup[3] == "1" || current_lookup[3] == "5") : (current_lookup[3] == "2" || current_lookup[3] == "4")))
                    return current_lookup[1];
            }

            return null;
        }

        private static bool Allowed_Element(string view, string id, ref Dictionary<string, Thing> dict)
        {
            Thing value;
            if(dict.TryGetValue(id, out value))
                return Allowed_Class(view, value.type);

            return false;
        }

        private static bool Allowed_Needline(string view, List<Thing> values, ref Dictionary<string, Thing> dict)
        {
            foreach (Thing thing in values)
            {
                if (thing.type == "activityPerformedByPerformer")
                    if (Allowed_Element(view, thing.place1, ref dict) == false)
                        return false;
            }
            
            return true;
        }

        private static bool Allowed_Class(string view, string type)
        {
            foreach (string[] current_lookup in Mandatory_Lookup)
            {
                if (current_lookup[1] != view)
                    continue;

                if (type == current_lookup[0])
                    return true;
            }

            foreach (string[] current_lookup in Optional_Lookup)
            {
                if (current_lookup[1] != view)
                    continue;

                if (type == current_lookup[0])
                    return true;
            }

            return false;
        }

        private static bool Proper_View(List<Thing> input, string name, string type, string id, ref List<string> errors)
        {
            bool found = true;
            bool test = true;
            int count = 0;
            
            foreach (string[] current_lookup in Mandatory_Lookup)  
            {
                if (current_lookup[1] != type)
                    continue;

                found = false;
                foreach (Thing thing in input)
                {
                    if (thing.type == current_lookup[0])
                    {
                        found = true;
                        break;
                    }    
                }

                if (found == false)
                {
                    errors.Add("Diagram error," + id + "," + name + "," + type + ",Missing Mandatory Element: " + current_lookup[0] + ".  It is possible that UPIA does not alow the creation all mandatory elements and/or a UPIA Stereotype was not assigned to the model element." + "\r\n");
                    found = false;
//*************************************************************************************************************************************************
//     BYPASSING THIS TEST FOR FALSE SINCE IN MOST CASES, IT IS NOT POSSIBLE TO CREATE ALL MANDATORY ELEMENTS IN UPIA FOR ANY VIEW.
//*************************************************************************************************************************************************
                    test = true ;
                    count++;
                }
            }
            return test;
        }

        private static string Find_Mandatory_Optional(string element, string name, string view, string id, ref List<string> errors)
        {

            foreach (string[] current_lookup in Mandatory_Lookup)
            {
                if (element == current_lookup[0] && view == current_lookup[1])
                    return "Mandatory";
            }

            foreach (string[] current_lookup in Optional_Lookup)
            {
                if (element == current_lookup[0] && view == current_lookup[1])
                    return "Optional";
            }

            errors.Add("Diagram error," + id + "," + name + "," + view + ",Element Ignored. Type Not Allowed: " + element + "\r\n");
            return "$none$";
        }

        private static void Add_Tuples(ref List<List<Thing>> input_list, ref List<List<Thing>> sorted_results, List<Thing> relationships, ref List<string> errors) 
        {
            //List<List<Thing>> sorted_results = input_list_new;
            bool place1 = false;
            bool place2 = false;
            Thing value;
            
            //foreach (List<Thing> old_view in input_list)
            for(int i=0;i<input_list.Count;i++)
            {
                List<Thing> things_view = new List<Thing>();
                List<Thing> new_view = new List<Thing>();
                List<Thing> other_view = new List<Thing>();
                Dictionary<string, Thing> dic;


                //if (old_view.Where(x => x.value != null).Where(x => (string)x.value == "$none$").Count() > 0)
                //    new_view = old_view.Where(x => x.value != null).Where(x => (string)x.value == "$none$").ToList();

                other_view = input_list[i].Where(x => x.value != null).Where(x => (string)x.value != "$none$" && ((string)x.value).Substring(0, 1) != "_").ToList();

                if (sorted_results.Count == i)
                {
                    
                    new_view.AddRange(input_list[i].Where(x => x.value == null).ToList());
                    new_view.AddRange(input_list[i].Where(x => x.value != null).Where(x => ((string)x.value).Substring(0, 1) == "_"));


                    foreach (Thing thing in other_view)
                    {
                        if (Find_Mandatory_Optional((string)thing.value, other_view.First().name, thing.type, thing.place1, ref errors) != "$none$")
                            new_view.Add(thing);
                    }

                    //remove
                    //var duplicateKeys = new_view.GroupBy(x => x.place2)
                    //        .Where(group => group.Count() > 1)
                    //        .Select(group => group.Key);

                    //List<string> test = duplicateKeys.ToList();

                    new_view = new_view.GroupBy(x => x.place2).Select(y => y.First()).ToList();

                    dic = new_view.Where(x => x.place2 != null).ToDictionary(x => x.place2, x => x);

                }
                else
                {
                    other_view = other_view.GroupBy(x => x.place2).Select(y => y.First()).ToList();

                    dic = other_view.Where(x => x.place2 != null).ToDictionary(x => x.place2, x => x);
                }
                
                
                foreach (Thing rela in relationships)
                {
                    place1 = false;
                    place2 = false;

                    if (dic.TryGetValue(rela.place1, out value))
                        place1 = true;

                    if (dic.TryGetValue(rela.place2, out value))
                        place2 = true;

                    if (place1 && place2)
                    {
                        new_view.Add(new Thing { place1 = value.place1, place2 = rela.id, value = rela.type, type = value.type, value_type="$none$" });
                    }
                }

                if (sorted_results.Count == i)
                    sorted_results.Add(new_view);
                else
                    sorted_results[i] = sorted_results[i].Union(new_view).ToList();
            }

           // return sorted_results;
        }

        private static List<Thing> Add_Places(Dictionary<string,Thing> things, List<Thing> values)
        {
            values = values.Distinct().ToList();
            IEnumerable<Thing> results = new List<Thing>(values);
            List<Thing> places = new List<Thing>();
            Thing value;

            foreach (Thing rela in values)
            {

                if(things.TryGetValue(rela.place1, out value))
                    places.Add(value);

                if (things.TryGetValue(rela.place2, out value))
                    places.Add(value);

            }

            results = results.Concat(places.Distinct());
            return results.ToList();
        }

        private static List<List<Thing>> Get_Tuples_place1(Thing input, IEnumerable<Thing> relationships)
        {
            List<Thing> results = new List<Thing>();

            foreach (Thing rela in relationships)
            {

                    if (input.id == rela.place1)
                    {
                        results.Add(new Thing { id = rela.id, type = Find_SA_Relationship_Type(rela.type, input.type,"1"), place1 = input.id, place2 = rela.place2, value = input, value_type = "$Thing$" });
                    }                    
            }

            return results.GroupBy(x => x.type).Select(group => group.Distinct().ToList()).ToList(); ;
        }

        private static List<List<Thing>> Get_Tuples_place2(Thing input, IEnumerable<Thing> relationships)
        {
            List<Thing> results = new List<Thing>();

            foreach (Thing rela in relationships)
            {

                if (input.id == rela.place2)
                {
                    results.Add(new Thing { id = rela.id, type = Find_SA_Relationship_Type(rela.type, input.type,"2"), place1 = input.id, place2 = rela.place1, value = input, value_type = "$Thing$" });
                }
            }

            return results.GroupBy(x => x.type).Select(group => group.Distinct().ToList()).ToList(); ;
        }

        private static List<List<Thing>> Get_Tuples_id(Thing input, IEnumerable<Thing> relationships)
        {
            List<Thing> results = new List<Thing>();

            foreach (Thing rela in relationships)
            {

                if (input.id == rela.id)
                {
                    results.Add(new Thing { id = rela.id, type = "performerTarget", place1 = input.id, place2 = rela.place1, value = input, value_type = "$Thing$" });
                    results.Add(new Thing { id = rela.id, type = "performerSource", place1 = input.id, place2 = rela.place2, value = input, value_type = "$Thing$" });
                }
            }

            return results.GroupBy(x => x.type).Select(group => group.Distinct().ToList()).ToList(); ;
        }

        ////////////////////
        ////////////////////

        public static bool RSA2PES(byte[] input, ref string output, ref string errors)
        {
            IEnumerable<Thing> things = new List<Thing>();
            IEnumerable<Thing> tuple_types = new List<Thing>();
            IEnumerable<Thing> tuples = new List<Thing>();
            IEnumerable<Thing> results;
            IEnumerable<Thing> results2;
            IEnumerable<Thing> results3; //added for RSA which has diagrams embedded in two places so a union is needed.
            IEnumerable<Thing> results4; //added for RSA which has diagrams embedded in two places so a union is needed.  This one holds subordinate diagrams to union.
            IEnumerable<Thing> results5; //
            IEnumerable<Thing> UPIAMap;
            IEnumerable<Thing> UPIAMapTemp;
            IEnumerable<Location> locations;
            List<View> views = new List<View>();
            List<Thing> mandatory_list = new List<Thing>();
            List<Thing> optional_list = new List<Thing>();
            string temp;
            Dictionary<string, List<Thing>> doc_blocks_data;
            Dictionary<string, string> diagrams;
            Dictionary<string, string> not_processed_diagrams;
            Dictionary<string, Thing> things_dic;
            Dictionary<string, Thing> values_dic;
            Dictionary<string, Thing> values_dic2;
            Dictionary<string, Thing> UPIAMap_dic;
            Dictionary<string, List<Thing>> doc_blocks_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> description_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV1_pic_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> needline_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> CV1_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> CV1_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> CV4_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> CV4_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> needline_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV2_support_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV2_support_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV4_support_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV2_support_mandatory_views_2 = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV4_support_optional_views_2 = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV5b_aro_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV6c_aro_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> OV5b_aro_optional_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> PV1_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> DIV3_optional = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> DIV3_mandatory = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> results_dic;
            Dictionary<string, List<Thing>> period_dic = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> datatype_mandatory_dic = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> datatype_optional_dic = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> aro;
            Dictionary<string, List<Thing>> aro2;
            XElement root = XElement.Load(new MemoryStream(input));
            List<List<Thing>> sorted_results = new  List<List<Thing>>();
            List<List<Thing>> sorted_results_new = new List<List<Thing>>();
            List<List<Thing>> view_holder = new List<List<Thing>>();
            bool representation_scheme = false;
            List<Thing> values = new List<Thing>();
            List<Thing> values2 = new List<Thing>();
            List<Thing> values3 = new List<Thing>();
            List<Thing> values4 = new List<Thing>();
            List<Thing> values5 = new List<Thing>();
            List<Thing> values6 = new List<Thing>();
            List<Thing> values7 = new List<Thing>();
            Thing value;
            Thing value2;
            int count = 0;
            int count2 = 0;
            bool add = false;
            bool test = true;
            bool upiafirst = true;
            List<string> errors_list = new List<string>();
            XNamespace xi = "http://www.omg.org/XMI";
            XNamespace uml = "http://www.eclipse.org/uml2/3.0.0/UML";
            XNamespace upia = "http:///schemas/UPIA/_7hv4kEc6Ed-f1uPQXF_0HA/563";



            //Diagram Type  

            //results3 =
            results = 
                from result in root.Elements(uml + "Model").Descendants("contents")//.Elements("eAnnotations").Elements("contents")
                where (string)result.Attribute(xi +"type") == "umlnotation:UMLDiagram"
                select new Thing
                            {
                                type = (string)result.Attribute("type"),
                                id = (string)result.Attribute(xi + "id"),
                                name = ((string)result.Attribute("name")).Replace("&", " And "),
                                value = "$none$",
                                place1 = "$none$",
                                place2 = "$none$",
                                foundation = "Thing",
                                value_type = "$none$"
                            };
           /* results4 =
                from result in root.Elements(uml + "Model").Elements("packagedElement").Elements("eAnnotations").Elements("contents")
                select new Thing
                {
                    type = (string)result.Attribute("type"),
                    id = (string)result.Attribute(xi + "id"),
                    name = ((string)result.Attribute("name")).Replace("&", " And "),
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "Thing",
                    value_type = "$none$"
                };

            results = results3.Union(results4); */

            //Build the type map - level 1  Pair IDs with the UPIA types.
            UPIAMap =
                            from result in root.Elements(upia + "EnterpriseModel")
                            select new Thing
                            {
                                type = "EnterpriseModel",
                                id = (string)result.Attribute("base_Package"),
                                name = "&none$",
                                value = "$none$",
                                place1 = "$none$",
                                place2 = "$none$",
                                foundation = "Thing",
                                value_type = "$none$"
                            };
            foreach (string[] current_lookup in UPIA_Element_Id_Prop)
            {
                UPIAMapTemp = 
                            from result in root.Elements(upia + current_lookup[0])
                            select new Thing
                            {
                                type = current_lookup[0],
                                id = (string)result.Attribute(current_lookup[1]),
                                name = (string)result.Attribute(xi + "id"),
                                value = current_lookup[2],
                                place1 = "$none$",
                                place2 = "$none$",
                                foundation = current_lookup[3],
                                value_type = "$none$"
                            };

                if (upiafirst)
                {
                   UPIAMap = UPIAMapTemp;
                   upiafirst = false;
                }
                else
                {
                   UPIAMap = UPIAMap.Union(UPIAMapTemp);
                }
            }
            //diagrams = View_Lookup.ToDictionary(x => x[1], x => x[0]);
            not_processed_diagrams = Not_Processed_View_Lookup.ToDictionary(x => x[1], x => x[0]);
            foreach (Thing thing in results)
            {
                //if (!diagrams.TryGetValue(thing.type, out temp))
                //{
                    if (not_processed_diagrams.TryGetValue(thing.type, out temp))
                    {
                        errors_list.Add("Diagram error," + thing.id + "," + thing.name + "," + temp + ", Type Not Allowed - Diagram Ignored: " + thing.type + "\r\n");
                    }
                    //else
                    //{
                    //    errors_list.Add("Diagram error," + thing.id + "," + thing.name + ",Unknown, Type Not Allowed - Diagram Ignored: " + thing.type + "\r\n");
                    //}
                //}
            }


            //Regular Things
            
            //for packaged elements - most of the things.
            foreach (Thing UPIAThing in UPIAMap)
            {
                results =
                    from result in root.Elements(uml + "Model").Descendants("packagedElement")
                    where ((string)result.Attribute(xi + "id")) == UPIAThing.id 
                    //where (string)result.Attribute(xi + "type") == current_lookup[1]
                    select new Thing
                    {
                        type = (string)UPIAThing.value,
                        id = (string)result.Attribute(xi + "id"),
                        name = ((string)result.Attribute("name")),//.Replace("&", " And ") ?? "$none$",
                        value = "$none$",
                        place1 = "$none$",
                        place2 = "$none$",
                        foundation = (string)UPIAThing.foundation,
                        value_type = "$none$"
                    };
                things = things.Concat(results.ToList());
            }

            //for non-packaged elements -those things that are contained in other things (such as tasks - which are operations of a class)
            foreach (Thing UPIAThing in UPIAMap)
            {
                results =
                    from result in root.Elements(uml + "Model").Descendants("ownedOperation")
                    where ((string)result.Attribute(xi + "id")) == UPIAThing.id
                    //where (string)result.Attribute(xi + "type") == current_lookup[1]
                    select new Thing
                    {
                        type = (string)UPIAThing.value,
                        id = (string)result.Attribute(xi + "id"),
                        name = (string)result.Attribute("name"),//.Replace("&", " And ") ?? "$none$",  //commnented out because looks like in RSA emx file that & is replace by &amp;
                        value = "$none$",
                        place1 = (string)result.Parent.Attribute(xi + "id"), //putting in the parent (owning) class here so I can figure out latter where activities should be added to views. Formerly "$none$"
                        place2 = "$none$",
                        foundation = (string)UPIAThing.foundation,
                        value_type = "$none$"
                    };
                things = things.Concat(results.ToList());
            }

            /*
            things = things.Concat(results.ToList());
            foreach(string[] current_lookup in Element_Lookup)
            {
                results =
                    from result in root.Elements(uml + "Model").Elements("packagedElement")
                    where (string)result.Attribute(xi + "type") == current_lookup[1]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = (string)result.Attribute(xi + "id"),
                        name = ((string)result.Attribute("name")).Replace("&", " And "),
                        value = "$none$",
                        place1 = "$none$",
                        place2 = "$none$",
                        foundation = current_lookup[2],
                        value_type = "$none$"
                    };

                things = things.Concat(results.ToList());
                
                if (current_lookup[1] != "Entity" && current_lookup[1] != "Access Path" && current_lookup[1] != "Index" && current_lookup[1] != "Table")
                {
                    results_dic =
                        (from result in root.Elements("Class").Elements("SADefinition").Elements("SAProperty")
                         where (string)result.Parent.Attribute("SAObjMinorTypeName") == current_lookup[1]
                         where (string)result.Attribute("SAPrpName") == "Description"
                         select new
                         {
                             key = (string)result.Parent.Attribute("SAObjId"),
                             value = new List<Thing> {
                            new Thing
                            {
                                type = "Information",
                                id = (string)result.Parent.Attribute("SAObjId") + "_9",
                                name = ((string)result.Parent.Attribute("SAObjName")).Replace("&", " And ") + " Description",
                                value = ((string)result.Attribute("SAPrpValue")).Replace("@", " At ").Replace("\"","'").Replace("&", " And "),
                                place1 = (string)result.Parent.Attribute("SAObjId"),
                                place2 = (string)result.Parent.Attribute("SAObjId") + "_9",
                                foundation = "IndividualType",
                                value_type = "exemplar"
                            }
                        }
                         }).ToDictionary(a => a.key, a => a.value);

                    things = things.Concat(results_dic.SelectMany(x => x.Value));

                    foreach (Thing thing in results_dic.SelectMany(x => x.Value))
                    {
                        value = new Thing
                        {
                            type = "describedBy",
                            id = thing.place1 + "_10",
                            foundation = "namedBy",
                            place1 = thing.place1,
                            place2 = thing.place2,
                            name = "$none$",
                            value = "$none$",
                            value_type = "$none$"
                        };
                        tuples = tuples.Concat(new List<Thing> { value });
                        description_views.Add(thing.place1, new List<Thing> { value });
                    }

                    MergeDictionaries(description_views, results_dic);
                }
                else if (current_lookup[1] == "Index")
                {
                    results =
                        from result in root.Elements("Class").Elements("SADefinition").Elements("SAProperty").Elements("SALink")
                         where (string)result.Parent.Parent.Attribute("SAObjMinorTypeName") == current_lookup[1]
                         where (string)result.Parent.Attribute("SAPrpName") == "Description"
                         
                         select new Thing
                            {
                                type = "Information",
                                id = (string)result.Parent.Parent.Attribute("SAObjId") + (string)result.Attribute("SALinkIdentity") + "_9",
                                name = ((string)result.Parent.Parent.Attribute("SAObjName")).Replace("&", " And ") + " Primary Key",
                                value = (string)result.Attribute("SALinkIdentity"),
                                place1 = (string)result.Parent.Parent.Attribute("SAObjId"),
                                place2 = (string)result.Parent.Parent.Attribute("SAObjId") + (string)result.Attribute("SALinkIdentity") + "_9",
                                foundation = "IndividualType",
                                value_type = "exemplar"
                            };

                    things = things.Concat(results);

                    sorted_results = results.GroupBy(x => x.place1).Select(group => group.ToList()).ToList();

                    foreach (List<Thing> view in sorted_results)
                    {
                        values = new List<Thing>();
                        foreach (Thing thing in view)
                        {
                            value = new Thing
                            {
                                type = "describedBy",
                                id = thing.place2 + "_10",
                                foundation = "namedBy",
                                place1 = thing.place1,
                                place2 = thing.place2,
                                name = "$none$",
                                value = "$none$",
                                value_type = "$none$"
                            };
                            tuples = tuples.Concat(new List<Thing> { value });
                            values.Add(value);
                            values.Add(thing);
                        }
                        description_views.Add(view.First().place1, values);
                    }

                    //MergeDictionaries(description_views, results_dic);
                } 
            } */

            //OV-1 Picture
/*
            results =
                from result in root.Elements("Class").Elements("SADiagram").Elements("SASymbol").Elements("SAPicture")
                where (string)result.Parent.Attribute("SAObjMinorTypeName") == "Picture"
                where (string)result.Parent.Parent.Attribute("SAObjMinorTypeName") == "OV-01 High Level Operational Concept (DM2)"
                select 
                //new {
                //    key = (string)result.Parent.Parent.Attribute("SAObjId"),
                //    value = new List<Thing> {
                        new Thing
                    {
                    type = "ArchitecturalDescription",
                    id = (string)result.Parent.Attribute("SAObjId"),
                    name = ((string)result.Parent.Attribute("SAObjName")).Replace("&", " And "),
                    value = (string)result.Attribute("SAPictureData"),
                    place1 = (string)result.Parent.Parent.Attribute("SAObjId"),
                    place2 = (string)result.Parent.Attribute("SAObjId"),
                    foundation = "IndividualType",
                    value_type = "exemplar"
                    };
                //}}).ToDictionary(a => a.key, a => a.value);

            OV1_pic_views = results.GroupBy(x=>x.place1).ToDictionary(x=>x.Key, x=>x.ToList());

            if (OV1_pic_views.Count() > 0)
            {
                representation_scheme = true;
                foreach (KeyValuePair<string, List<Thing>> entry in OV1_pic_views)
                {
                    foreach (Thing thing in entry.Value)
                    {
                        tuples = tuples.Concat(new List<Thing>{new Thing
                            {
                            type = "representationSchemeInstance",
                            id = thing.id+"_1",
                            name = "$none$",
                            value = "$none$",
                            place1 = "_rs1",
                            place2 = thing.id,
                            foundation = "typeInstance",
                            value_type = "$none$"
                            }});
                    }
                }
                things = things.Concat(OV1_pic_views.SelectMany(x => x.Value));
            }
*/
            //Regular tuples
/*
            foreach (string[] current_lookup in Tuple_Lookup)
            {
                if (current_lookup[3] == "1")
                {
                    results =
                        from result in root.Elements("Class").Elements("SADefinition").Elements("SAProperty").Elements("SALink")
                        where (string)result.Parent.Attribute("SAPrpName") == current_lookup[1]
                        select new Thing
                        {
                            type = current_lookup[0],
                            id = (string)result.Parent.Parent.Attribute("SAObjId") + (string)result.Attribute("SALinkIdentity"),
                            name = "$none$",
                            value = "$none$",
                            place1 = (string)result.Parent.Parent.Attribute("SAObjId"),
                            place2 = (string)result.Attribute("SALinkIdentity"),
                            foundation = current_lookup[2],
                            value_type = "$none$"
                        };
                }
                else
                {
                    results =
                        from result in root.Elements("Class").Elements("SADefinition").Elements("SAProperty").Elements("SALink")
                        where (string)result.Parent.Attribute("SAPrpName") == current_lookup[1]
                        select new Thing
                        {
                            type = current_lookup[0],
                            id = (string)result.Attribute("SALinkIdentity") + (string)result.Parent.Parent.Attribute("SAObjId"),
                            name = "$none$",
                            value = "$none$",
                            place2 = (string)result.Parent.Parent.Attribute("SAObjId"),
                            place1 = (string)result.Attribute("SALinkIdentity"),
                            foundation = current_lookup[2],
                            value_type = "$none$"
                        };
                }
                tuples = tuples.Concat(results.ToList());
            }

            tuples = tuples.GroupBy(x => x.id).Select(grp => grp.First());
*/
            //Regular TupleTypes

            foreach (string[] current_lookup in Tuple_Type_Lookup)
            {
                if (current_lookup[3] == "uml:Association" && current_lookup[2] != "ExercisesCapability") //member end contains both place1 and place2
                {
                    results =
                        from result in root.Elements(uml + "Model").Descendants("packagedElement")
                        where (string)result.Attribute(xi + "type") == current_lookup[3]
                        where result.Attribute(current_lookup[4]) != null && ((string)result.Attribute(current_lookup[4])).Length > 24
                        from result4 in root.Elements(uml + "Model").Descendants("ownedAttribute")
                        where (string)result4.Attribute(xi + "id") == ((string)result.Attribute(current_lookup[4])).Substring(0, 23)
                        from result5 in root.Elements(uml + "Model").Descendants("ownedAttribute")
                        where (string)result5.Attribute(xi + "id") == ((string)result.Attribute(current_lookup[4])).Substring(24)
                        //from result6 in root.Elements(uml + "Model").Descendants("ownedEnd")
                        //where (string)result6.Attribute("association") == ((string)result.Attribute(current_lookup[4])).Substring(0, 23)
                        select new Thing
                        {
                            type = current_lookup[1],
                            id = (string)result.Attribute(xi + "id"),
                            name = "$none$",
                            value = "$none$",
                            place1 = (string)result4.Attribute("type"), 
                            place2 = (string)result5.Attribute("type"), 
                            foundation = current_lookup[0],
                            value_type = "$none$"
                        };

                    tuple_types = tuple_types.Concat(results.ToList());

                }
                else if (current_lookup[3] == "uml:Association" && current_lookup[2] == "ExercisesCapability") //member end contains both place1 and place2
                {
                    results =
                        from result in root.Elements(uml + "Model").Descendants("packagedElement")
                        where (string)result.Attribute(xi + "type") == current_lookup[3]
                        where result.Attribute(current_lookup[4]) != null && ((string)result.Attribute(current_lookup[4])).Length > 24
                        from result4 in root.Elements(upia + current_lookup[2])
                        where (string)result4.Attribute("base_Association") == (string)result.Attribute(xi + "id")  //check that the association is a UPIA Exercises Capability stereotype
                        from result5 in root.Elements(uml + "Model").Descendants("ownedAttribute")
                        where (string)result5.Attribute(xi + "id") == ((string)result.Attribute(current_lookup[4])).Substring(24)
                        from result6 in root.Elements(uml + "Model").Descendants("ownedEnd")
                        where (string)result6.Attribute(xi + "id") == ((string)result.Attribute(current_lookup[4])).Substring(0, 23)
                        select new Thing
                        {
                            type = current_lookup[1],
                            id = (string)result.Attribute(xi + "id"),
                            name = "$none$",
                            value = "$none$",
                            place1 = (string)result6.Attribute("type"),
                            place2 = (string)result5.Attribute("type"),
                            foundation = current_lookup[0],
                            value_type = "$none$"
                        };

                    tuple_types = tuple_types.Concat(results.ToList());

                }
                else
                {
                    results =
                        from result in root.Elements(uml + "Model").Descendants("packagedElement")
                        where (string)result.Attribute(xi + "type") == current_lookup[3]
                        where result.Attribute(current_lookup[4]) != null && result.Attribute(current_lookup[5]) != null
                        select new Thing
                        {
                            type = current_lookup[1],
                            id = (string)result.Attribute(xi + "id"),
                            name = "$none$",
                            value = "$none$",
                            place1 = (string)result.Attribute(current_lookup[4]),
                            place2 = (string)result.Attribute(current_lookup[5]),
                            foundation = current_lookup[0],
                            value_type = "$none$"
                        };

                    tuple_types = tuple_types.Concat(results.ToList());

                }
            }

            results =
                from result in root.Elements(upia + "DataExchange")
                from result4 in root.Elements(uml + "Model").Descendants("packagedElement")
                where (string)result4.Attribute(xi + "id") == (string)result.Attribute("base_InformationFlow")
                select new Thing
                {
                    type = "activityProducesResource",
                    id =  (string)result.Attribute(xi + "id") + "_1",
                    name = (string)result.Attribute("exchangeId"),
                    place1 = (string)result.Attribute("producingTask"),
                    place2 = (string)result4.Attribute("conveyed"),
                    foundation = "CoupleType",
                    value_type = "exemplar"
                };
            tuple_types = tuple_types.Concat(results.ToList());

            results =
                from result in root.Elements(upia + "DataExchange")
                from result4 in root.Elements(uml + "Model").Descendants("packagedElement")
                where (string)result4.Attribute(xi + "id") == (string)result.Attribute("base_InformationFlow")
                select new Thing
                {
                    type = "activityConsumesResource",
                    id = (string)result.Attribute(xi + "id") + "_2",
                    name = (string)result.Attribute("exchangeId"),
                    place2 = (string)result.Attribute("consumingTask"),
                    place1 = (string)result4.Attribute("conveyed"),
                    foundation = "CoupleType",
                    value_type = "exemplar"
                };
            tuple_types = tuple_types.Concat(results.ToList());

            //Activityperformedbyperformer - variation where the performer is a class and the operation a task identified in the UPIA stereotype area.
            foreach (Thing UPIAThing in UPIAMap)
            {
                results =
                    from result in root.Elements(uml + "Model").Descendants("ownedOperation")
                    where ((string)result.Attribute(xi + "id")) == UPIAThing.id
                    //where (string)result.Attribute(xi + "type") == current_lookup[1]
                    select new Thing
                    {
                        type = "activityPerformedByPerformer",
                        id = (string)result.Attribute(xi + "id") + "_3",
                        name = (string)result.Attribute("name"),//.Replace("&", " And ") ?? "$none$",  //commnented out because looks like in RSA emx file that & is replace by &amp;
                        value = "$none$",
                        place1 = (string)result.Parent.Attribute(xi + "id"), 
                        place2 = (string)result.Attribute(xi + "id"),
                        foundation = "CoupleType",
                        value_type = "exemplar"
                    };
                tuple_types = tuple_types.Concat(results.ToList());
            }

            tuple_types = tuple_types.GroupBy(x => x.id).Select(grp => grp.First());

            tuple_types = tuple_types.Distinct();

            things = things.Distinct();

            //Milestone Date
            // NEEDS WORK
            results =
                    from result in root.Elements(upia + "ActualMeasure")
                    select new Thing
                    {
                        type = "HappensInType",
                        id = (string)result.Attribute(xi + "id"),
                        name = "$none$",
                        value = "$none$",
                        place1 = (string)result.Attribute("base_InstanceSpecification"),
                        place2 = ((string)result.Attribute("measuredElements")).Substring(0, 23),
                        foundation = "WholePartType",
                        value_type = "$period$"
                    };

            tuple_types = tuple_types.Concat(results.ToList());

            //things_dic  put in some code to compenate for non-unique keys.
            IEnumerable<Thing> filteredthings = things.GroupBy(x => x.id).Select(group => group.First());
            things_dic = filteredthings.ToDictionary(x => x.id, x => x);
           // things_dic = things.ToDictionary(x => x.id, x => x);

            //System Exchange (DM2rx)

            //ToLists
                    values3 = tuples.ToList();
                    values4 = tuple_types.ToList();
                    things = null;
                    tuples = null;
                    tuple_types = null;




            //Diagramming
/***********************************************************************************************************************************************************************
            locations =
                    from result in root.Elements("Class").Elements("SADiagram").Elements("SASymbol")
                    where (string)result.Attribute("SASymIdDef") != null
                        || (string)result.Attribute("SAObjMinorTypeName") == "Picture" || (string)result.Attribute("SAObjMinorTypeName") == "Doc Block"
                    select new Location
                    {
                        id = ((string)result.Attribute("SAObjMinorTypeName") == "Picture" || (string)result.Attribute("SAObjMinorTypeName") == "Doc Block") ? (string)result.Parent.Attribute("SAObjId") + (string)result.Attribute("SAObjId") : (string)result.Parent.Attribute("SAObjId") + (string)result.Attribute("SAObjId"),
                        top_left_x = (string)result.Attribute("SASymLocX"),
                        top_left_y = (string)result.Attribute("SASymLocY"),
                        bottom_right_x = ((int)result.Attribute("SASymLocX") + (int)result.Attribute("SASymSizeX")).ToString(),
                        bottom_right_y = ((int)result.Attribute("SASymLocY") - (int)result.Attribute("SASymSizeY")).ToString(),
                        element_id = ((string)result.Attribute("SAObjMinorTypeName") == "Picture" || (string)result.Attribute("SAObjMinorTypeName") == "Doc Block") ? (string)result.Attribute("SAObjId") : (string)result.Attribute("SASymIdDef")
                    };

            foreach (Location location in locations)
            {
                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "Information",
                    id = location.id + "_12",
                    name = "Diagramming Information",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "Point",
                    id = location.id + "_16",
                    name = "Top Left Location",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "PointType",
                    id = location.id + "_14",
                    name = "Top Left LocationType",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "Point",
                    id = location.id + "_26",
                    name = "Bottome Right Location",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "PointType",
                    id = location.id + "_24",
                    name = "Bottome Right LocationType",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_18",
                    name = "Top Left X Location",
                    value = location.top_left_x,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue",
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_20",
                    name = "Top Left Y Location",
                    value = location.top_left_y,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_22",
                    name = "Top Left Z Location",
                    value = "0",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_28",
                    name = "Bottom Right X Location",
                    value = location.bottom_right_x,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_30",
                    name = "Bottom Right Y Location",
                    value = location.bottom_right_y,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_32",
                    name = "Bottom Right Z Location",
                    value = "0",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values5.AddRange(values);

                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "describedBy",
                    id = location.id + "_11",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.element_id,
                    place2 = location.id + "_12",
                    foundation = "namedBy",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "typeInstance",
                    id = location.id + "_15",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_14",
                    place2 = location.id + "_16",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "typeInstance",
                    id = location.id + "_25",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_24",
                    place2 = location.id + "_26",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });


                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_17",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_18",
                    place2 = location.id + "_16",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_19",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_20",
                    place2 = location.id + "_16",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_21",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_22",
                    place2 = location.id + "_16",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_27",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_28",
                    place2 = location.id + "_26",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_29",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_30",
                    place2 = location.id + "_26",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_31",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_32",
                    place2 = location.id + "_26",
                    foundation = "typeInstance",
                    value_type = "$none$"
                });

                values7.AddRange(values);

                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "resourceInLocationType",
                    id = location.id + "_13",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_12",
                    place2 = location.id + "_14",
                    foundation = "CoupleType",
                    value_type = "$none$"
                });

                values.Add(new Thing
                {
                    type = "resourceInLocationType",
                    id = location.id + "_23",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_12",
                    place2 = location.id + "_24",
                    foundation = "CoupleType",
                    value_type = "$none$"
                });


                values6.AddRange(values);
            }
*/
            locations = null;
            values = null;
            values2 = null;
            values_dic = null;
            values_dic2 = null;
            results2 = null;

            //Views

            foreach (string[] current_lookup in View_Lookup)
            {
                sorted_results = new List<List<Thing>>();

               /* if (current_lookup[1]=="Usecase")
                {
                results = 
                    from result in root.Elements(uml + "Model").Elements("packagedElement").Elements("eAnnotations").Elements("contents").Elements("children")
                    where (string)result.Attribute(xi + "type") != "notation:Shape"                                                                             //Need some way of differentiating Use Case diagrams since they are not ALL CV-6 related
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = (string)result.Parent.Attribute(xi + "id") + (string)result.Attribute("element"),
                        name = ((string)result.Parent.Attribute("name")).Replace("&", " And "),
                        place1 = (string)result.Parent.Attribute(xi + "id"),
                        place2 = (string)result.Attribute("element"),
                        value = (string)result.Attribute("element"),
                        foundation = "$none$",
                        value_type = "$element_type$"
                    };
                }
                else
                {*/
                results =
                    from result in root.Elements(uml + "Model").Descendants()//.Elements("children")
                    where (string)result.Attribute(xi + "type") == "umlnotation:UMLShape" || (string)result.Attribute(xi + "type") == "umlnotation:UMLClassifierShape"
                    from result2 in result.Ancestors()
                    where (string)result2.Attribute("type") == current_lookup[1]
                    where result2.Attribute("name") != null && ((string)result2.Attribute("name")).Contains((string)current_lookup[0])  //no differing DoDAF diagram types so have to check for name containment.
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = (string)result2.Attribute(xi + "id") + (string)result.Attribute("element"), //combined ids
                        name = ((string)result2.Attribute("name")).Replace("&", " And "),                //diagram name  
                        place1 = (string)result2.Attribute(xi + "id"),
                        place2 = (string)result.Attribute("element"),
                        value = (string)result.Attribute("element"),
                        foundation = "$none$",
                        value_type = "$element_type$"
                    };
              //  }
                
                //view_holder.Add(results.ToList());

                //Add Operations to views
                values = new List<Thing>();
                values = things_dic.Select(kvp => kvp.Value).ToList();
                foreach (Thing UPIATHING in values)
                {
                    if ((string)UPIATHING.type == "Activity" && (string)UPIATHING.place1 != "$none$")
                    {
                            results2 =
                                from result in root.Elements(uml + "Model").Descendants()//.Elements("children")
                                where (string)result.Attribute(xi + "type") == "umlnotation:UMLShape" || (string)result.Attribute(xi + "type") == "umlnotation:UMLClassifierShape"
                                where (string)result.Attribute("element") == (string)UPIATHING.place1
                                from result2 in result.Ancestors()
                                where (string)result2.Attribute("type") == current_lookup[1]
                                where result2.Attribute("name") != null && ((string)result2.Attribute("name")).Contains((string)current_lookup[0])  //no differing DoDAF diagram types so have to check for name containment.
                                select new Thing
                                {
                                    type = current_lookup[0],
                                    id = (string)result2.Attribute(xi + "id") + UPIATHING.id, //combined ids
                                    name = ((string)result2.Attribute("name")).Replace("&", " And "),                //diagram name  
                                    place1 = (string)result2.Attribute(xi + "id"),
                                    place2 = (string)UPIATHING.id,
                                    value = (string)UPIATHING.id,
                                    foundation = "$none$",
                                    value_type = "$element_type$"
                                };
                            //view_holder.Add(results.ToList());
                            results = results.Concat(results2.ToList());
                    }
                }
                values = null;

                //Add Resources to views
                values = new List<Thing>();
                values = things_dic.Select(kvp => kvp.Value).ToList();
                foreach (Thing UPIATHING in values)
                {
                    if ((string)UPIATHING.type == "Data" )
                    {
                            results3 =
                                from result in root.Elements(upia + "DataExchange")
                                from result4 in root.Elements(uml + "Model").Descendants("packagedElement")
                                where (string)result4.Attribute(xi + "id") == (string)result.Attribute("base_InformationFlow")
                                where (string)result4.Attribute("conveyed") == (string)UPIATHING.id
                                from result3 in root.Elements(uml + "Model").Descendants("edges")
                                where (string)result3.Attribute("element") == (string)result4.Attribute(xi + "id") 
                                where (string)result3.Attribute(xi + "type") == "umlnotation:UMLConnector"
                                from result2 in result3.Ancestors()
                                where (string)result2.Attribute("type") == current_lookup[1]
                                where result2.Attribute("name") != null && ((string)result2.Attribute("name")).Contains((string)current_lookup[0])  //no differing DoDAF diagram types so have to check for name containment.

                                select new Thing
                                {
                                    type = current_lookup[0],
                                    id = (string)result2.Attribute(xi + "id") + UPIATHING.id, //combined ids
                                    name = ((string)result2.Attribute("name")).Replace("&", " And "),                //diagram name  
                                    place1 = (string)result2.Attribute(xi + "id"),
                                    place2 = (string)UPIATHING.id,
                                    value = (string)UPIATHING.id,
                                    foundation = "$none$",
                                    value_type = "$element_type$"
                                };
                            //view_holder.Add(results.ToList());
                            results = results.Concat(results3.ToList());
                    }
                }
                values = null;

                view_holder.Add(results.ToList());
            }
            root = null;
            values = null;

            foreach (List<Thing> view_elements in view_holder)
            {
                //foreach (Thing thing in values)
                int max = view_elements.Count;
                for (int i = 0; i < max; i++)
                {
                    Thing thing = view_elements[i];
                    //thing.value = (string) Find_Def_DM2_Type((string)thing.value, values5.ToList());
                    if (thing.place2 != null)
                    {
                        if (things_dic.TryGetValue(thing.place2, out value))
                            thing.value = (string)value.type;
                        /*
                        if (thing.type == "DIV-3")
                        {
                            values2 = new List<Thing>();
                            
                            if (results_dic.TryGetValue(thing.place2, out values2))
                            {
                                foreach (Thing item in values2)
                                {
                                    view_elements.Add(new Thing
                                    {
                                        type = thing.type,
                                        id = thing.id,
                                        name = thing.name,
                                        value = thing.value,
                                        place1 = thing.place1,
                                        place2 = item.place2,
                                        foundation = thing.foundation,
                                        value_type = thing.value_type
                                    });
                                    max++;
                                }
                            }
                         }*/
                    }
                }

                sorted_results = view_elements.GroupBy(x => x.place1).Select(group => group.Distinct().ToList()).ToList();

                sorted_results_new = new List<List<Thing>>();
                Add_Tuples(ref sorted_results, ref sorted_results_new, values3, ref errors_list);
                Add_Tuples(ref sorted_results, ref sorted_results_new, values4, ref errors_list);
                sorted_results = sorted_results_new;

                foreach (List<Thing> view in sorted_results)
                {

                    if (view.Count() == 0)
                        continue;

                    mandatory_list = new List<Thing>();
                    optional_list = new List<Thing>();
                    /*
                    if (view.First().type == "CV-1")
                    {
                        if (CV1_mandatory_views.TryGetValue(view.First().place1, out values))
                            mandatory_list.AddRange(values);
                    }

                    if (view.First().type == "PV-1")
                    {
                        values = new List<Thing>();
                        if (PV1_mandatory_views.TryGetValue(view.First().place1, out values))
                            mandatory_list.AddRange(values);
                    }
                    */
                    foreach (Thing thing in view)
                    {
                        if (thing.place2 != null)
                        {
                            if (((string)thing.value).Substring(0, 1) != "_")
                            {
                                temp = Find_Mandatory_Optional((string)thing.value, view.First().name, thing.type, thing.place1, ref errors_list);
                                if (temp == "Mandatory")
                                {
                                    mandatory_list.Add(new Thing { id = thing.place2, type = (string)thing.value, value = "$none$", value_type = "$none$" });
                                }
                                if (temp == "Optional")
                                {
                                    optional_list.Add(new Thing { id = thing.place2, type = (string)thing.value, value = "$none$", value_type = "$none$" });
                                }
                            }
                            /*
                            values = new List<Thing>();
                            if (needline_mandatory_views.TryGetValue(thing.place2, out values))
                            {
                                if (Allowed_Needline(thing.type, values, ref things_dic) == true)
                                {
                                    mandatory_list.AddRange(values);
                                    //if (!view.First().type.Contains("SV-4") && !view.First().type.Contains("SvcV-4") && !view.First().type.Contains("SV-10b"))
                                    if (needline_optional_views.TryGetValue(thing.place2, out values2))
                                        optional_list.AddRange(values2);
                                        //optional_list.AddRange(needline_optional_views[thing.place2]);
                                }
                            }
                            */
                            values = new List<Thing>();
                            if (description_views.TryGetValue(thing.place2, out values))
                                optional_list.AddRange(values);

                            values = new List<Thing>();
                            if (period_dic.TryGetValue(thing.place2, out values))
                                optional_list.AddRange(values);

                            values = new List<Thing>();
                            if (datatype_mandatory_dic.TryGetValue(thing.place2, out values))
                                mandatory_list.AddRange(values);

                            values = new List<Thing>();
                            if (datatype_optional_dic.TryGetValue(thing.place2, out values))
                                optional_list.AddRange(values);
                            /*
                            if (thing.type.Contains("SvcV"))
                            {
                                if ((string)thing.value == "Service")
                                {
                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "ServiceDescription",
                                        id = thing.place2 + "_2",
                                        name = thing.place2 + "_Description",
                                        value = "$none$",
                                        place1 = "$none$",
                                        place2 = "$none$",
                                        foundation = "Individual",
                                        value_type = "$none$"
                                    });

                                    values.Add(new Thing
                                    {
                                        type = "serviceDescribedBy",
                                        id = thing.place2 + "_1",
                                        name = "$none$",
                                        value = "$none$",
                                        place1 = thing.id,
                                        place2 = thing.id + "_2",
                                        foundation = "namedBy",
                                        value_type = "$none$"
                                    });

                                    mandatory_list.AddRange(values);

                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "ServiceDescription",
                                        id = thing.place2 + "_2",
                                        name = thing.place2 + "_Description",
                                        value = "$none$",
                                        place1 = "$none$",
                                        place2 = "$none$",
                                        foundation = "Individual",
                                        value_type = "$none$"
                                    });

                                    values5.AddRange(values);

                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "serviceDescribedBy",
                                        id = thing.place2 + "_1",
                                        name = "$none$",
                                        value = "$none$",
                                        place1 = thing.id,
                                        place2 = thing.id + "_2",
                                        foundation = "namedBy",
                                        value_type = "$none$"
                                    });

                                    values7.AddRange(values);
                                }
                            }*/
                            /*
                            else if (thing.type == "OV-6c")
                            {
                                values = new List<Thing>();
                                if (OV6c_aro_optional_views.TryGetValue(thing.place2, out values))
                                {
                                    optional_list.AddRange(values);
                                }
                            }
                            else if (thing.type == "OV-5b" || thing.type == "OV-6b")
                            {
                                values = new List<Thing>();
                                if (OV5b_aro_optional_views.TryGetValue(thing.place2, out values))
                                {
                                    optional_list.AddRange(values);
                                    mandatory_list.AddRange(OV5b_aro_mandatory_views[thing.place2]);
                                }
                            }
                            else if (thing.type == "DIV-3")
                            {
                                values = new List<Thing>();
                                if (DIV3_optional.TryGetValue(thing.place2, out values))
                                {
                                    optional_list.AddRange(values);
                                }
                                values = new List<Thing>();
                                if (DIV3_mandatory.TryGetValue(thing.place2, out values))
                                {
                                    mandatory_list.AddRange(values);
                                }
                            }
                            else if (thing.type == "OV-4")
                            {
                                values = new List<Thing>();
                                if (OV4_support_optional_views.TryGetValue(thing.place2, out values))
                                    if (Allowed_Class("OV-4",(string)thing.value))
                                        optional_list.AddRange(values);
                            }
                            else if (thing.type == "OV-2")
                            {
                                values = new List<Thing>();
                                if (OV2_support_mandatory_views.TryGetValue(thing.place2, out values))
                                    if (Allowed_Class("OV-2", (string)thing.value))
                                        mandatory_list.AddRange(values);
                                values = new List<Thing>();
                                if (OV2_support_optional_views.TryGetValue(thing.place2, out values))
                                    if (Allowed_Class("OV-2", (string)thing.value))
                                        optional_list.AddRange(values);
                            }
                            
                            else if (thing.type == "AV-1" || thing.type.Contains("PV"))
                            {
                                if ((string)thing.value == "ProjectType")
                                {
                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "typeInstance",
                                        id = thing.place2 + "_1",
                                        name = "$none$",
                                        value = "$none$",
                                        place1 = thing.id,
                                        place2 = thing.id + "_2",
                                        foundation = "typeInstance",
                                        value_type = "$none$"
                                    });

                                    optional_list.AddRange(values);

                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "Project",
                                        id = thing.place2 + "_2",
                                        name = thing.place2 + "_Project",
                                        value = "$none$",
                                        place1 = "$none$",
                                        place2 = "$none$",
                                        foundation = "Individual",
                                        value_type = "$none$"
                                    });

                                    mandatory_list.AddRange(values);

                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "typeInstance",
                                        id = thing.place2 + "_1",
                                        name = "$none$",
                                        value = "$none$",
                                        place1 = thing.id,
                                        place2 = thing.id + "_2",
                                        foundation = "typeInstance",
                                        value_type = "$none$"
                                    });

                                    values7.AddRange(values);

                                    values = new List<Thing>();

                                    values.Add(new Thing
                                    {
                                        type = "Project",
                                        id = thing.place2 + "_2",
                                        name = thing.place2 + "_Project",
                                        value = "$none$",
                                        place1 = "$none$",
                                        place2 = "$none$",
                                        foundation = "Individual",
                                        value_type = "$none$"
                                    });

                                    values5.AddRange(values);
                                }

                                if (thing.type == "PV-1")
                                {
                                    if ((string)thing.value == "OrganizationType")
                                    {
                                        values = new List<Thing>();

                                        values.Add(new Thing
                                        {
                                            type = "typeInstance",
                                            id = thing.place2 + "_1",
                                            name = "$none$",
                                            value = "$none$",
                                            place1 = thing.id,
                                            place2 = thing.id + "_2",
                                            foundation = "typeInstance",
                                            value_type = "$none$"
                                        });

                                        optional_list.AddRange(values);

                                        values = new List<Thing>();

                                        values.Add(new Thing
                                        {
                                            type = "Organization",
                                            id = thing.place2 + "_2",
                                            name = thing.place2 + "_Organization",
                                            value = "$none$",
                                            place1 = "$none$",
                                            place2 = "$none$",
                                            foundation = "Individual",
                                            value_type = "$none$"
                                        });

                                        mandatory_list.AddRange(values);

                                        values = new List<Thing>();

                                        values.Add(new Thing
                                        {
                                            type = "typeInstance",
                                            id = thing.place2 + "_1",
                                            name = "$none$",
                                            value = "$none$",
                                            place1 = thing.id,
                                            place2 = thing.id + "_2",
                                            foundation = "typeInstance",
                                            value_type = "$none$"
                                        });

                                        values7.AddRange(values);

                                        values = new List<Thing>();

                                        values.Add(new Thing
                                        {
                                            type = "Organization",
                                            id = thing.place2 + "_2",
                                            name = thing.place2 + "_Organization",
                                            value = "$none$",
                                            place1 = "$none$",
                                            place2 = "$none$",
                                            foundation = "Individual",
                                            value_type = "$none$"
                                        });

                                        values5.AddRange(values);
                                    }
                                }
                            }
                            else if (thing.type == "CV-1")
                            {
                                values = new List<Thing>();
                                if (CV1_mandatory_views.TryGetValue(thing.place2, out values))
                                    mandatory_list.AddRange(values);
                                values = new List<Thing>();
                                if (CV1_optional_views.TryGetValue(thing.place2, out values))
                                    optional_list.AddRange(values);
                            }
                            else if (thing.type == "CV-4")
                            {
                                values = new List<Thing>();
                                if (CV4_mandatory_views.TryGetValue(thing.place2, out values))
                                    mandatory_list.AddRange(values);
                                values = new List<Thing>();
                                if (CV4_optional_views.TryGetValue(thing.place2, out values))
                                    optional_list.AddRange(values);
                            }*/
                          
                        }
                    }

                    mandatory_list = mandatory_list.GroupBy(x => x.id).Select(grp => grp.First()).ToList();
                    optional_list = optional_list.GroupBy(x => x.id).Select(grp => grp.First()).ToList();

                    values = new List<Thing>();
                    if(doc_blocks_views.TryGetValue(view.First().place1, out values))
                        optional_list.AddRange(values);

                    values = new List<Thing>();
                    if (OV1_pic_views.TryGetValue(view.First().place1, out values))
                            mandatory_list.AddRange(values);

                    mandatory_list = mandatory_list.OrderBy(x => x.type).ToList();
                    optional_list = optional_list.OrderBy(x => x.type).ToList();

                    if (Proper_View(mandatory_list, view.First().name,view.First().type, view.First().place1, ref errors_list))
                        views.Add(new View { type = view.First().type, id = view.First().place1, name = view.First().name, mandatory = mandatory_list, optional = optional_list });
                    //else
                    //{
                    //    test = false;
                    //}
                }
            }

            results_dic = null;
            mandatory_list = null;
            optional_list = null;
            view_holder = null;

            using (var sw = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {

                    writer.WriteRaw(@"<IdeasEnvelope OriginatingNationISO3166TwoLetterCode=""String"" ism:ownerProducer=""NMTOKEN"" ism:classification=""U""
                    xsi:schemaLocation=""http://cio.defense.gov/xsd/dm2 DM2_PES_v2.02_Chg_1.XSD""
                    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ism=""urn:us:gov:ic:ism:v2"" xmlns:ideas=""http://www.ideasgroup.org/xsd""
                    xmlns:dm2=""http://www.ideasgroup.org/dm2""><IdeasData XMLTagsBoundToNamingScheme=""DM2Names"" ontologyVersion=""2.02_Chg_1"" ontology=""DM2"">
		            <NamingScheme ideas:FoundationCategory=""NamingScheme"" id=""ns1""><ideas:Name namingScheme=""ns1"" id=""NamingScheme"" exemplarText=""DM2Names""/>
		            </NamingScheme>");
                    writer.WriteRaw("\n");
                    if (representation_scheme)
                    {
                        writer.WriteRaw(@"<RepresentationScheme ideas:FoundationCategory=""Type"" id=""id_rs1"">
			            <ideas:Name id=""RepresentationScheme"" namingScheme=""ns1"" exemplarText=""Base64 Encoded Image""/>
		                </RepresentationScheme>");
                        writer.WriteRaw("\n");
                    }

                    values = things_dic.Select(kvp => kvp.Value).ToList();
                    things_dic = null;
                    foreach (Thing thing in values)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id + "\" "
                            + (((string)thing.value_type).Contains("$") ? "" : thing.value_type + "=\"" + (string)thing.value + "\"") + ">" + "<ideas:Name exemplarText=\"" + thing.name
                            + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n");
                    values = null;

                    foreach (Thing thing in values5)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id + "\" "
                            + (((string)thing.value_type).Contains("$") ? "" : thing.value_type + "=\"" + (string)thing.value + "\"") + ">" + "<ideas:Name exemplarText=\"" + thing.name
                            + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n");
                    values5 = null;

                    foreach (Thing thing in values4)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" place1Type=\"id" + thing.place1 + "\" place2Type=\"id" + thing.place2 + "\""
                        + (((string)thing.name).Contains("$") ? "/>\n" : ">" + "<ideas:Name exemplarText=\"" + thing.name
                        + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n"));
                    values4 = null;

                    foreach (Thing thing in values6)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" place1Type=\"id" + thing.place1 + "\" place2Type=\"id" + thing.place2 + "\""
                        + (((string)thing.name).Contains("$") ? "/>\n" : ">" + "<ideas:Name exemplarText=\"" + thing.name
                        + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n"));
                    values6 = null;

                    foreach (Thing thing in values3)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" tuplePlace1=\"id" + thing.place1 + "\" tuplePlace2=\"id" + thing.place2 + "\""
                        + (((string)thing.name).Contains("$") ? "/>\n" : ">" + "<ideas:Name exemplarText=\"" + thing.name
                        + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n"));
                    values3 = null;

                    foreach (Thing thing in values7)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" tuplePlace1=\"id" + thing.place1 + "\" tuplePlace2=\"id" + thing.place2 + "\""
                        + (((string)thing.name).Contains("$") ? "/>\n" : ">" + "<ideas:Name exemplarText=\"" + thing.name
                        + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">\n"));
                    values7 = null;

                    writer.WriteRaw("</IdeasData>\n");

                    writer.WriteRaw("<IdeasViews frameworkVersion=\"DM2.02_Chg_1\" framework=\"DoDAF\">\n");

                    foreach (View view in views)
                    {
                        writer.WriteRaw("<" + view.type + " id=\"id" + view.id + "\" name=\"" + view.name + "\">\n");

                        writer.WriteRaw("<MandatoryElements>\n");

                        foreach (Thing thing in view.mandatory)
                        {
                            writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>\n");
                        }

                        writer.WriteRaw("</MandatoryElements>\n");
                        writer.WriteRaw("<OptionalElements>\n");

                        foreach (Thing thing in view.optional)
                        {
                            writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>\n");
                        }

                        writer.WriteRaw("</OptionalElements>\n");
                        writer.WriteRaw("</" + view.type + ">\n");
                    }

                    views = null;

                    writer.WriteRaw("</IdeasViews>\n");

                    writer.WriteRaw("</IdeasEnvelope>\n");

                    writer.Flush();
                }

                output = sw.ToString();
                errors = string.Join("", errors_list.Distinct().ToArray());

                if (errors.Count() > 0)
                    test = false;

                return test;
            }
        }

        ////////////////////
        ////////////////////

        public static string PES2SA(byte[] input)
        {
            Dictionary<string,Thing> things = new Dictionary<string,Thing>();
            Dictionary<string, Thing> results_dic;
            Dictionary<string, Location> location_dic = new Dictionary<string, Location>();
            IEnumerable<Thing> tuple_types = new List<Thing>();
            IEnumerable<Thing> tuples = new List<Thing>();
            IEnumerable<Thing> results;
            List<View> views = new List<View>();
            string temp="";
            string temp2="";
            string temp3="";
            string date = DateTime.Now.ToString("d");
            string time = DateTime.Now.ToString("T");
            string prop_date = DateTime.Now.ToString("yyyyMMdd");
            string prop_time = DateTime.Now.ToString("HHmmss");
            string minor_type;
            string minor_type_name;
            Guid view_GUID;
            Guid thing_GUID;
            Guid temp_GUID;
            Dictionary<string, Guid> thing_GUIDs = new Dictionary<string, Guid>();
            Dictionary<string, Thing> OV1_pic_views;
            Dictionary<string, List<Thing>> CV4_CD_views;
            Dictionary<string, List<Thing>> ARO_views;
            Dictionary<string, Thing> doc_block_views;
            Dictionary<string, List<Thing>> support_views;
            Dictionary<string, List<Thing>> needline_views;
            List<string> SA_Def_elements = new List<string>();
            XElement root = XElement.Load(new MemoryStream(input));
            List<List<Thing>> sorted_results;
            //bool representation_scheme = false;
            int count = 0;
            int count2 = 0;
            string loc_x, loc_y, size_x, size_y;
            Thing value;
            List<Thing> values;
            XNamespace ns = "http://www.ideasgroup.org/xsd";
            XNamespace xi = "http://www.omg.org/XMI";
            XNamespace um = "http://www.omg.org/UML";
            Location location;
            List<string> errors_list = new List<string>();

            // regular Things

            foreach (string[] current_lookup in Element_Lookup)
            {
                if (current_lookup[5] != "default")
                    continue;
                //if (current_lookup[0] == "ArchitecturalDescription")
                //{
                //    results =
                //      from result in root.Elements("Class").Elements("SADiagram").Elements("SASymbol").Elements("SAPicture")
                //      where (string)result.Parent.Attribute("SAObjMinorTypeName") == "Picture"
                //      where (from diagram in result.Parent.Parent.Parent.Elements("SADefinition")
                //             where (string)diagram.Attribute("SAObjId") == (string)result.Parent.Attribute("SASymIdDef")
                //             select diagram
                //         ).Any()
                //      select new Thing
                //      {
                //          type = "ArchitecturalDescription",
                //          id = (string)result.Parent.Attribute("SASymIdDef"),
                //          name = (string)result.Parent.Attribute("SAObjName"),
                //          value = (string)result.Attribute("SAPictureData"),
                //          place1 = "$none$",
                //          place2 = "$none$",
                //          foundation = "IndividualType",
                //          value_type = "exemplar"
                //      };

                //    //if (results.Count() > 0)
                //        //representation_scheme = true;
                //}
                //else
                //{

                results =
                    from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                    where (string)result.Parent.Name.ToString() == current_lookup[0]
                    select new Thing
                        {
                            type = current_lookup[0],
                            id = ((string)result.Parent.Attribute("id")).Substring(2),
                            name = (string)result.Attribute("exemplarText"),
                            value = current_lookup[1],
                            place1 = "$none$",
                            place2 = "$none$",
                            foundation = (string)result.Parent.Attribute(ns + "FoundationCategory"),
                            value_type = "SAObjMinorTypeName"
                        };

                results_dic =
                    (from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                     where (string)result.Parent.Name.ToString() == current_lookup[0]
                     select new
                     {
                         key = ((string)result.Parent.Attribute("id")).Substring(2),
                         value = new Thing
                         {
                             type = current_lookup[0],
                             id = ((string)result.Parent.Attribute("id")).Substring(2),
                             name = (string)result.Attribute("exemplarText"),
                             value = current_lookup[1],
                             place1 = "$none$",
                             place2 = "$none$",
                             foundation = (string)result.Parent.Attribute(ns + "FoundationCategory"),
                             value_type = "SAObjMinorTypeName"
                         }
                     }).ToDictionary(a => a.key, a => a.value);
                //}

                if (results_dic.Count() > 0)
                    MergeDictionaries(things, results_dic);
            }

            //  diagramming

            results =
                     from result in root.Elements("IdeasData").Elements("SpatialMeasure").Elements(ns + "Name")
                     select new Thing
                         {
                             id = ((string)result.Parent.Attribute("id")).Substring(2,((string)result.Parent.Attribute("id")).Length-5),
                             name = (string)result.Attribute("exemplarText"),
                             value = (string)result.Parent.Attribute("numericValue"),
                             place1 = "$none$",
                             place2 = "$none$",
                             foundation = "$none$",
                             value_type = "diagramming"
                         };

            sorted_results = results.GroupBy(x => x.id).Select(group => group.OrderBy(x => x.name).ToList()).ToList();

            foreach (List<Thing> coords in sorted_results)
            {
                location_dic.Add(coords.First().id, 
                    new Location {
                        id = coords.First().id,
                        bottom_right_x = (string)coords[0].value,
                        bottom_right_y = (string)coords[1].value,
                        bottom_right_z ="0",
                        top_left_x = (string)coords[3].value,
                        top_left_y = (string)coords[4].value,
                        top_left_z = "0",
                    });
            }

            // doc block

            results =
                    from result in root.Elements("IdeasData").Elements("Information")
                    from result2 in root.Elements("IdeasData").Elements("describedBy")
                    where ((string)result2.Attribute("tuplePlace2")).Substring(2) == ((string)result.Attribute("id")).Substring(2)
                    select new Thing
                    {
                          type = "Information",
                          id = ((string)result.Attribute("id")).Substring(2),
                          name = (string)result.Attribute("exemplar"),
                          value = "$none$",
                          place1 = "$none$",
                          place2 = "$none$",
                          foundation = "IndividualType",
                          value_type = "$none$"
                      };
            if (results.Count() > 0)
            {
                foreach (Thing thing in results)
                {
                    things.Remove(thing.id);
                }
            }

            doc_block_views =
                   (from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                    where (string)result.Attribute("exemplarText") == "Doc Block Comment"
                    from result2 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                    where (string)result2.Parent.Parent.Name.ToString() != "AV-2"
                    where ((string)result2.Attribute("ref")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                    select new
                    {
                        key = ((string)result2.Parent.Parent.Attribute("id")).Substring(2),
                        value = new Thing
                        {
                            type = "$none$",
                            id = ((string)result.Parent.Attribute("id")).Substring(2),
                            name = (string)result.Attribute("exemplarText"),
                            value = ((string)result.Parent.Attribute("exemplar")),
                            place1 = "$none$",
                            place2 = "$none$",
                            foundation = "$none$",
                            value_type = "Comment"
                        }
                    }).ToDictionary(a => a.key, a => a.value);

            //Support

            results =
                     from result5 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                     where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_1"
                     where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_2"
                     from result in root.Elements("IdeasData").Elements("activityPerformedByPerformer")
                     where ((string)result5.Attribute("ref")).Substring(2) == ((string)result.Attribute("place1Type")).Substring(2)
                     from result2 in root.Elements("IdeasData").Elements("activityConsumesResource").Elements(ns + "Name")
                     where ((string)result2.Attribute("exemplarText") == "Support")
                     where ((string)result.Parent.Attribute("place2Type")).Substring(2) == ((string)result2.Parent.Attribute("place2Type")).Substring(2)
                     from result6 in root.Elements("IdeasData").Elements("Resource")
                     where ((string)result6.Attribute("id")).Substring(2) == ((string)result2.Parent.Attribute("place1Type")).Substring(2)
                     from result3 in root.Elements("IdeasData").Elements("activityProducesResource")
                     where ((string)result2.Parent.Attribute("place1Type")).Substring(2) == ((string)result3.Attribute("place2Type")).Substring(2)
                     from result4 in root.Elements("IdeasData").Elements("activityPerformedByPerformer")
                     where ((string)result3.Attribute("place1Type")).Substring(2) == ((string)result4.Attribute("place2Type")).Substring(2)
                     
                     select new Thing
                    {
                        type = "SupportedBy",
                        id = ((string)result.Attribute("place1Type")).Substring(2) + ((string)result4.Attribute("place1Type")).Substring(2),
                        name = "$none$",
                        value = ((string)result5.Parent.Parent.Attribute("id")).Substring(2),
                        place1 = ((string)result.Attribute("place1Type")).Substring(2),
                        place2 = ((string)result4.Attribute("place1Type")).Substring(2),
                        foundation = "$none$",
                        value_type = "View ID"
                    };

            support_views = results.GroupBy(x => (string)x.value)
                             .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            if (results.Count() > 0)
            {
                foreach (Thing thing in results)
                {
                    things.Remove(thing.place1 + "_2");
                    things.Remove(thing.place2 + "_2");
                    things.Remove(thing.place1 + "_3");
                    things.Remove(thing.place2 + "_3");
                }
            }

            // Needlines and System Resource Flow

            results =
                     from result5 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                     where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_1"
                     where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_2"
                     from result in root.Elements("IdeasData").Elements("activityPerformedByPerformer").Elements(ns + "Name")
                     where ((string)result5.Attribute("ref")).Substring(2) == ((string)result.Parent.Attribute("place1Type")).Substring(2)
                     from result2 in root.Elements("IdeasData").Elements("activityConsumesResource").Elements(ns + "Name")
                     where ((string)result2.Attribute("exemplarText") == "Needline") || ((string)result2.Attribute("exemplarText") == "SF")
                     where ((string)result.Parent.Attribute("place2Type")).Substring(2) == ((string)result2.Parent.Attribute("place2Type")).Substring(2)
                     from result6 in root.Elements("IdeasData").Elements("Resource")
                     where ((string)result6.Attribute("id")).Substring(2) == ((string)result2.Parent.Attribute("place1Type")).Substring(2)
                     from result3 in root.Elements("IdeasData").Elements("activityProducesResource")
                     where ((string)result2.Parent.Attribute("place1Type")).Substring(2) == ((string)result3.Attribute("place2Type")).Substring(2)
                     from result4 in root.Elements("IdeasData").Elements("activityPerformedByPerformer")
                     where ((string)result3.Attribute("place1Type")).Substring(2) == ((string)result4.Attribute("place2Type")).Substring(2)
                     
                     select new Thing
                     {
                         type = 
                         Resource_Flow_Type(
                         (string)result2.Attribute("exemplarText"), (string)result5.Parent.Parent.Name.ToString(), ((string)result.Parent.Attribute("place1Type")).Substring(2), ((string)result4.Attribute("place1Type")).Substring(2), things
                         ),
                         id = ((string)result2.Attribute("id")).Substring(1,((string)result2.Attribute("id")).Length-3),
                         name = ((string)result.Attribute("exemplarText")),
                         value = ((string)result5.Parent.Parent.Attribute("id")).Substring(2),
                         place1 = ((string)result.Parent.Attribute("place1Type")).Substring(2),
                         place2 = ((string)result4.Attribute("place1Type")).Substring(2),
                         foundation = "$none$",
                         value_type = "View ID"
                     };

            needline_views = results.GroupBy(x => (string)x.value)
                             .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            if (results.Count() > 0)
            {
                foreach (Thing thing in needline_views.First().Value)
                {
                    things.Remove(thing.id + "_1");
                    things.Remove(thing.id + "_2");
                    things.Remove(thing.id + "_4");
                    things.Add(thing.id,thing);
                }
            }

            // Capability Dependency

            results =
                   from result2 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                   where (string)result2.Name.ToString() == "CV-4_BeforeAfterType"
                   from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                   where ((string)result2.Attribute("ref")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                   select new Thing
                       {
                           type = "Capability Dependency (DM2rx)",
                           id = ((string)result.Parent.Attribute("id")).Substring(2),
                           name = (string)result.Attribute("exemplarText"),
                           value = ((string)result2.Parent.Parent.Attribute("id")).Substring(2),
                           place1 = ((string)result.Parent.Attribute("place1Type")).Substring(2),
                           place2 = ((string)result.Parent.Attribute("place2Type")).Substring(2),
                           foundation = "$none$",
                           value_type = "View ID"
                       };
                
            CV4_CD_views = results.GroupBy(x => (string)x.value)
                             .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            
            if (CV4_CD_views.Count() > 0)
            {
                foreach (Thing thing in CV4_CD_views.First().Value)
                {
                    things.Remove(thing.id);
                }
            }

            //ARO

            results =
                   from result5 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                   where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_1"
                   where ((string)result5.Parent.Parent.Attribute("id")).Substring(2) != "_2"
                   from result2 in root.Elements("IdeasData").Elements("activityConsumesResource").Elements(ns + "Name")
                   where ((string)result2.Attribute("exemplarText") == "ARO")
                   where ((string)result5.Attribute("ref")).Substring(2) == ((string)result2.Parent.Attribute("place1Type")).Substring(2)
                   from result6 in root.Elements("IdeasData").Elements("Resource").Elements(ns + "Name")
                   where ((string)result6.Parent.Attribute("id")).Substring(2) == ((string)result2.Parent.Attribute("place1Type")).Substring(2)
                   from result3 in root.Elements("IdeasData").Elements("activityProducesResource")
                   where ((string)result2.Parent.Attribute("place1Type")).Substring(2) == ((string)result3.Attribute("place2Type")).Substring(2)
                   select new Thing
                   {
                       type = "ActivityResourceOverlap (DM2r)",
                       id = ((string)result3.Attribute("id")).Substring(2, ((string)result3.Attribute("id")).Length - 4),
                       name = ((string)result6.Attribute("exemplarText")),
                       value = ((string)result5.Parent.Parent.Attribute("id")).Substring(2),
                       place1 = ((string)result3.Attribute("place1Type")).Substring(2),
                       place2 = ((string)result2.Parent.Attribute("place2Type")).Substring(2),
                       foundation = "$none$",
                       value_type = "View ID"
                   };

            ARO_views = results.GroupBy(x => (string)x.value)
                             .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            if (ARO_views.Count() > 0)
            {
                foreach (Thing thing in ARO_views.First().Value)
                {
                    things.Remove(thing.id + "_1");
                    things.Remove(thing.id + "_2");
                    things.Remove(thing.id + "_3");
                }
            }

            // OV-1 Pic

            OV1_pic_views =
                   (
                    from result2 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                    where (string)result2.Name.ToString() == "OV-1_ArchitecturalDescription"
                    from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                    where ((string)result2.Attribute("ref")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                    from result3 in root.Elements("IdeasData").Elements("representationSchemeInstance")
                    //where (string)result.Parent.Name.ToString() == "ArchitecturalDescription"
                    where ((string)result3.Attribute("tuplePlace2")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                    select new
                    {
                        key = ((string)result2.Parent.Parent.Attribute("id")).Substring(2),
                        value = new Thing
                        {
                            type = "ArchitecturalDescription",
                            id = ((string)result.Parent.Attribute("id")).Substring(2),
                            name = (string)result.Attribute("exemplarText"),
                            value = ((string)result.Parent.Attribute("exemplar")),
                            place1 = "$none$",
                            place2 = "$none$",
                            foundation = (string)result.Parent.Attribute(ns + "FoundationCategory"),
                            value_type = "Picture"
                        }
                    }).ToDictionary(a => a.key, a => a.value);

            if (OV1_pic_views.Count() > 0)
            {
                foreach (Thing thing in OV1_pic_views.Values.ToList())
                {
                    things.Remove(thing.id);
                }
            }

            // regular tuples

            foreach (string[] current_lookup in Tuple_Lookup)
            {
                if (current_lookup[3] != "1" && current_lookup[3] != "5")
                    continue;

                results =
                    from result in root.Elements("IdeasData").Descendants()
                    where (string)result.Name.ToString() == current_lookup[0]
                    from result2 in root.Elements("IdeasData").Descendants()
                    where ((string)result.Attribute("tuplePlace1")) == ((string)result2.Attribute("id"))
                    where (string)result2.Name.ToString() == current_lookup[5]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Attribute("id")).Substring(2),
                        name = "$none$",
                        value = (string)result2.Name.ToString(),
                        place1 = ((string)result.Attribute("tuplePlace1")).Substring(2),
                        place2 = ((string)result.Attribute("tuplePlace2")).Substring(2),
                        foundation = current_lookup[2],
                        value_type = "element type"
                    };

                tuples = tuples.Concat(results.ToList());
            }

            // regular tuple types

            foreach (string[] current_lookup in Tuple_Type_Lookup)
            {

                if (current_lookup[3] != "1" && current_lookup[3] != "5")
                    continue;

                results =
                    from result in root.Elements("IdeasData").Descendants()
                    where (string)result.Name.ToString() == current_lookup[0]
                    from result2 in root.Elements("IdeasData").Descendants()
                    where ((string)result.Attribute("place1Type")) == ((string)result2.Attribute("id"))
                    where (string)result2.Name.ToString() == current_lookup[5]

                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Attribute("id")).Substring(2),
                        name = "$none$",
                        value = (string)result2.Name.ToString(),
                        place1 = ((string)result.Attribute("place1Type")).Substring(2),
                        place2 = ((string)result.Attribute("place2Type")).Substring(2),
                        foundation = current_lookup[2],
                        value_type = "element type"
                    };

                tuple_types = tuple_types.Concat(results.ToList());
            }

            // views

            foreach (string[] current_lookup in View_Lookup)
            {
                if (current_lookup[3] != "default")
                    continue;
                results =
                    from result in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                    where (string)result.Parent.Parent.Name.ToString() == current_lookup[0]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Parent.Parent.Attribute("id")).Substring(2) + ((string)result.Attribute("ref")).Substring(2),
                        name = ((string)result.Parent.Parent.Attribute("name")).Replace("&", " And "),
                        place1 = ((string)result.Parent.Parent.Attribute("id")).Substring(2),
                        place2 = ((string)result.Attribute("ref")).Substring(2),
                        value = (things.TryGetValue(((string)result.Attribute("ref")).Substring(2), out value)) ? value : new Thing {type="$none$"},
                        foundation = "$none$",
                        value_type = "Thing"
                    };


                sorted_results = results.GroupBy(x => x.name).Select(group => group.Distinct().ToList()).ToList();
                //sorted_results = Add_Tuples(sorted_results, tuples);
                //sorted_results = Add_Tuples(sorted_results, tuple_types);

                foreach (List<Thing> view in sorted_results)
                {
                    List<Thing> mandatory_list = new List<Thing>();
                    List<Thing> optional_list = new List<Thing>();

                    foreach (Thing thing in view)
                    {

                        temp = Find_Mandatory_Optional((string)((Thing)thing.value).type, view.First().name, thing.type, thing.place1, ref errors_list);
                        if (temp == "Mandatory")
                        {
                            mandatory_list.Add(new Thing { id = thing.place2, name = (string)((Thing)thing.value).name, type = (string)((Thing)thing.value).value });
                        }
                        if (temp == "Optional")
                        {
                            optional_list.Add(new Thing { id = thing.place2, name = (string)((Thing)thing.value).name, type = (string)((Thing)thing.value).value });
                        }
                    }

                    mandatory_list = mandatory_list.OrderBy(x => x.type).ToList();
                    optional_list = optional_list.OrderBy(x => x.type).ToList();

                    if (needline_views.TryGetValue(view.First().place1, out values))
                        optional_list.AddRange(values);

                    if (CV4_CD_views.TryGetValue(view.First().place1, out values))
                        optional_list.AddRange(values);

                    if (ARO_views.TryGetValue(view.First().place1, out values))
                        optional_list.AddRange(values);

                    //if (Proper_View(mandatory_list, view.First().type))
                    views.Add(new View { type = current_lookup[1], id = view.First().place1, name = view.First().name, mandatory = mandatory_list, optional = optional_list });
                }
            }

            // output

            foreach (string thing in things.Keys)
            {
                    thing_GUID = Guid.NewGuid();
                    thing_GUIDs.Add(thing, thing_GUID);
            }

            using (var sw = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {

                    writer.WriteRaw(@"<Classes>");

                    foreach (View view in views)
                    {
                        count2 = 0;
                        count++;
                        view_GUID = Guid.NewGuid();
                        minor_type = Find_View_SA_Minor_Type(view.type);

                        writer.WriteRaw("<Class><SADiagram SAObjId=\"" + view.id + "\" SAObjName=\"" + view.name + "\" SAObjMinorTypeName=\"" + view.type
                            + "\" SAObjMinorTypeNum=\"" + minor_type + "\" SAObjMajorTypeNum=\"1\" SAObjAuditId=\"NEAR\" SAObjUpdateDate=\""
                            + date + "\" SAObjUpdateTime=\"" + time + "\" SAObjFQName=\"" + view.name + "\" "
                            + "SADgmCLevelNumber=\"\" SADgmSnapGridEnt=\"0\" SADgmSnapGridLin=\"0\" SADgmPGridNumEnt=\"4 4\" SADgmPGridNumLin=\"10 10\""
                            + " SADgmPGridSizeEnt=\"25 25\" SADgmPGridSizeLin=\"10 10\" SADgmGridUnit100=\"100 100\" SADgmBPresentationMenu=\"0\""
                            + " SADgmBShowPages=\"0\" SADgmBShowRuler=\"0\" SADgmBShowGrid=\"-1\" SADgmBShowScroll=\"-1\" SADgmBShowNodeShadow=\"-1\""
                            + " SADgmBShowLineShadow=\"0\" SADgmBShowTextShadow=\"0\" SADgmPShadowDelta=\"5 5\" SADgmRGBShadowColor=\"0x00c0c0c0\""
                            + " SADgmRMargin=\"50 50 50 50\" SADgmBBorder=\"0\" SADgmBorderOffset=\"-13\" SADgmWBorderPenStyle=\"0x0010\" SADgmBDgmBorder=\"0\""
                            + " SADgmIDgmForm=\"0\" SADgmWOrientation=\"0x0003\" SADgmBDgmPDefault=\"1\" SADgmBIsHierarchy=\"0\" SADgmBBackgroundColorOn=\"0\""
                            + " SADgmRGBBackgroundColor=\"0x00ffffff\" SADgmWLinePenStyle=\"0x0103\">");

                        writer.WriteRaw("<SAProperty SAPrpName=\"~C~\" SAPrpValue=\"1\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                            + "<SAProperty SAPrpName=\"~T~\" SAPrpValue=\"" + minor_type + "\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                            + "<SAProperty SAPrpName=\"Use Automatic Gradient Fills\" SAPrpValue=\"T\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                            + "<SAProperty SAPrpName=\"DGX File Name\" SAPrpValue=\"D" + count.ToString("D7") + ".DGX\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                            //+ ((minor_type == "283") ? "" : "<SAProperty SAPrpName=\"Hierarchical Numbering\" SAPrpValue=\"F\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>") 
                            + "<SAProperty SAPrpName=\"Initial Date\" SAPrpValue=\"" + prop_date + "\" SAPrpEditType=\"2\" SAPrpLength=\"10\"/>"
                            + "<SAProperty SAPrpName=\"Initial Time\" SAPrpValue=\"" + prop_time + "\" SAPrpEditType=\"7\" SAPrpLength=\"11\"/>"
                            + "<SAProperty SAPrpName=\"Initial Audit\" SAPrpValue=\"NEAR\" SAPrpEditType=\"1\" SAPrpLength=\"8\"/>"
                            + "<SAProperty SAPrpName=\"GUID\" SAPrpValue=\"" + view_GUID + "\" SAPrpEditType=\"1\" SAPrpLength=\"64\"/>"
                            // + "<SAProperty SAPrpName=\"Description\" SAPrpValue=\"\" SAPrpEditType=\"1\" SAPrpLength=\"4074\"/>"
                            //+ "<SAProperty SAPrpName=\"Vertical Pools and Lanes\" SAPrpValue=\"F\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                            //+ "<SAProperty SAPrpName=\"Check Connections\" SAPrpValue=\"F\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                            //+ "<SAProperty SAPrpName=\"Auto-create/update 1380\" SAPrpValue=\"T\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                            //+ "<SAProperty SAPrpName=\"Auto-populate Where of APBP\" SAPrpValue=\"T\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                            //+ "<SAProperty SAPrpName=\"Peers\" SAPrpValue=\"\" SAPrpEditType=\"14\" SAPrpLength=\"1200\" SAPrpEditDefMajorType=\"Diagram\"" 
                            //+ " SAPrpEditDefMinorType=\"" + view.type + "\"/>"
                            //+ "<SAProperty SAPrpName=\"Architecture Type\" SAPrpValue=\"\" SAPrpEditType=\"1\" SAPrpLength=\"1200\"/>"
                            //+ "<SAProperty SAPrpName=\"Related Architecture Description\" SAPrpValue=\"\" SAPrpEditType=\"14\" SAPrpLength=\"1200\""
                            //+ " SAPrpEditDefMajorType=\"Definition\" SAPrpEditDefMinorType=\"ArchitecturalDescription (DM2)\"/>"
                            //+ "<SAProperty SAPrpName=\"OSLCLink\" SAPrpValue=\"\" SAPrpEditType=\"8\" SAPrpLength=\"4074\" SAPrpEditDefMajorType=\"Definition\""
                            //+ " SAPrpEditDefMinorType=\"OSLC Link\"/>"
                            //+ "<SAProperty SAPrpName=\"Reference Documents\" SAPrpValue=\"\" SAPrpEditType=\"18\" SAPrpLength=\"1024\"/>"
                            + "<SAProperty SAPrpName=\"SA VISIO Last Modified By\" SAPrpValue=\"SA\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                            + "<SAProperty SAPrpName=\"Last Change Date\" SAPrpValue=\"" + DateTime.Now.ToString("yyyyMMdd") + "\" SAPrpEditType=\"2\" SAPrpLength=\"10\"/>"
                            + "<SAProperty SAPrpName=\"Last Change Time\" SAPrpValue=\"" + DateTime.Now.ToString("HHmmss") + "\" SAPrpEditType=\"7\" SAPrpLength=\"11\"/>"
                            + "<SAProperty SAPrpName=\"Last Change Audit\" SAPrpValue=\"NEAR\" SAPrpEditType=\"1\" SAPrpLength=\"8\"/>");

                        List<Thing> thing_list = new List<Thing>(view.mandatory);
                        thing_list.AddRange(view.optional);

                        foreach (Thing thing in thing_list)
                        {

                            if (thing_GUIDs.TryGetValue(thing.id, out thing_GUID) == false)
                            {
                                thing_GUID = Guid.NewGuid();
                                thing_GUIDs.Add(thing.id, thing_GUID);
                            }

                            if (location_dic.TryGetValue(view.id + thing.id, out location) == true)
                            {
                                loc_x = location.top_left_x;
                                loc_y = location.top_left_y;
                                size_x = (Convert.ToInt32(location.bottom_right_x) - Convert.ToInt32(location.top_left_x)).ToString();
                                size_y = (Convert.ToInt32(location.top_left_y) - Convert.ToInt32(location.bottom_right_y)).ToString();
                            }
                            else
                            {
                                loc_x = "574";
                                loc_y = "203";
                                size_x = "125";
                                size_y = "55";
                            }

                            minor_type_name = thing.type;
                            minor_type = Find_Symbol_Element_SA_Minor_Type(ref minor_type_name, view.type);

                            writer.WriteRaw("<SASymbol SAObjId=\"" + thing.id + view.id.Substring(1) + "\" SAObjName=\"" + thing.name + "\" SAObjMinorTypeName=\"" + minor_type_name + "\""
                                + " SAObjMinorTypeNum=\"" + minor_type + "\" SAObjMajorTypeNum=\"2\" SAObjAuditId=\"NEAR\" SAObjUpdateDate=\"" + date + "\""
                                + " SAObjUpdateTime=\"" + time + "\" SAObjFQName=\"&quot;" + thing_GUID + "&quot;.&quot;" + thing.name + "&quot;\" SASymIdDgm=\"" + view.id + "\" SASymIdDef=\"" + thing.id + "\""
                                //other
                                + " SASymArrangement=\"0\" SASymOtherSymbology=\"0\" SASymProperties=\"0x0000\" SASymOrder=\"0\" SASymXPEntity=\"" + count2 + "\""
                                + " SASymXPLink=\"65535\" SASymXPGroup=\"65535\" SASymXPSibling=\"65535\" SASymXPSubordinate=\"65535\" SASymPenStyle=\"0x0010\""
                                + " SASymFontName=\"\" SASymFontHeight=\"0x0000\" SASymFontFlags=\"0x0000\" SASymLineStyle=\"0x0103\" SASymFlags=\"0x0002\""
                                + " SASymFlags2=\"0x0000\" SASymFlags3=\"0x0000\" SASymTextFlags=\"0x082a\" SASymStyle=\"0\" SASymAuxStyle=\"0x0000\""
                                + " SASymOccurs=\"0x01\" SASymOccOffset=\"0x00\" SASymBGColor=\"0x00\" SASymFGColor=\"0x00\" SASymPrompt=\"0x00\""
                                + " SASymFrExArcChar=\"0x00\" SASymToExArcChar=\"0x00\" SASymUncleCount=\"0x00\" SASymStyleFlags=\"0x0007\" SASymSeqNum=\"0\""
                                + " SASymRotation=\"0\" SASymError1=\"0x00\" SASymError2=\"0x00\" SASymHideProgeny=\"0\" SASymHidden=\"0\" SASymOtherForm=\"0\""
                                + " SASymHasDspMode=\"0\" SASymDspMode=\"0x0000\" SASymDspModeExt=\"0x00000000\" SASymCLevelNumber=\"0\" SASymPenColorOn=\"1\""
                                + " SASymPenColorRed=\"0\" SASymPenColorGreen=\"130\" SASymPenColorBlue=\"236\" SASymFillColorOn=\"1\" SASymFillColorRed=\"176\""
                                + " SASymFillColorGreen=\"213\" SASymFillColorBlue=\"255\" SASymFontColorOn=\"1\" SASymFontColorRed=\"0\" SASymFontColorGreen=\"0\""
                                + " SASymFontColorBlue=\"0\" SASymLocX=\"" + loc_x + "\" SASymLocY=\"" + loc_y + "\" SASymSizeX=\"" + size_x + "\" SASymSizeY=\"" + size_y + "\" SASymNameLocX=\"572\""
                                + " SASymNameLocY=\"168\" SASymNameSizeX=\"121\" SASymNameSizeY=\"18\" SASymDescLocX=\"0\" SASymDescLocY=\"0\" SASymDescSizeX=\"0\""
                                + " SASymDescSizeY=\"0\">");

                            writer.WriteRaw("<SAProperty SAPrpName=\"~C~\" SAPrpValue=\"2\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                + "<SAProperty SAPrpName=\"~T~\" SAPrpValue=\"" + minor_type + "\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                + "<SAProperty SAPrpName=\"Object Class Number\" SAPrpValue=\"3\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                + "<SAProperty SAPrpName=\"Object Type Number\" SAPrpValue=\"" + Find_Definition_Element_SA_Minor_Type(thing.type) + "\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                + "<SAProperty SAPrpName=\"Symbol Represents\" SAPrpValue=\"" + thing.type + "\" SAPrpEditType=\"1\" SAPrpLength=\"4074\"/>"
                                + "<SAProperty SAPrpName=\"KeyGUID\" SAPrpValue=\"" + thing_GUID + "\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                //+ "<SARelation SARelId=\"_1982\" SARelTypeNum=\"6\" SARelTypeName=\"connects\"/>"
                                //+ "<SARelation SARelId=\"_1980\" SARelTypeNum=\"8\" SARelTypeName=\"connects\"/>"
                                //+ "<SARelation SARelId=\"_1979\" SARelTypeNum=\"28\" SARelTypeName=\"embeds\"/>"
                                //+ "<SARelation SARelId=\"_1991\" SARelTypeNum=\"28\" SARelTypeName=\"embeds\"/>"
                                + "</SASymbol>");
                            
                            count2++;
                        }

                        if (OV1_pic_views.TryGetValue(view.id, out value))
                        {
                            
                            if (location_dic.TryGetValue(view.id + value.id, out location) == true)
                            {
                                loc_x = location.top_left_x;
                                loc_y = location.top_left_y;
                                size_x = (Convert.ToInt32(location.bottom_right_x) - Convert.ToInt32(location.top_left_x)).ToString();
                                size_y = (Convert.ToInt32(location.top_left_y) - Convert.ToInt32(location.bottom_right_y)).ToString();
                            }
                            else
                            {
                                loc_x = "574";
                                loc_y = "203";
                                size_x = "125";
                                size_y = "55";
                            }

                            writer.WriteRaw("<SASymbol SAObjId=\"" + value.id + "\" SAObjName=\"" + value.name + "\" SAObjMinorTypeName=\"Picture\" SAObjMinorTypeNum=\"11\" SAObjMajorTypeNum=\"2\" SAObjAuditId=\"ir\""
                                + " SAObjUpdateDate=\"2/5/2015\" SAObjUpdateTime=\"10:00:16 AM\" SAObjFQName=\"&quot;&quot;\" SASymIdDgm=\"" + view.id + "\" SASymArrangement=\"0\" SASymOtherSymbology=\"0\""
                                + " SASymProperties=\"0x0000\" SASymOrder=\"0\" SASymXPEntity=\"1\" SASymXPLink=\"65535\" SASymXPGroup=\"65535\" SASymXPSibling=\"65535\" SASymXPSubordinate=\"65535\""
                                + " SASymPenStyle=\"0x0010\" SASymFontName=\"\" SASymFontHeight=\"0x0000\" SASymFontFlags=\"0x0000\" SASymLineStyle=\"0x0103\" SASymFlags=\"0x0002\" SASymFlags2=\"0x0000\""
                                + " SASymFlags3=\"0x0000\" SASymTextFlags=\"0x003a\" SASymStyle=\"0\" SASymAuxStyle=\"0x0000\" SASymOccurs=\"0x01\" SASymOccOffset=\"0x00\" SASymBGColor=\"0x00\" SASymFGColor=\"0x00\""
                                + " SASymPrompt=\"0x00\" SASymFrExArcChar=\"0x00\" SASymToExArcChar=\"0x00\" SASymUncleCount=\"0x00\" SASymStyleFlags=\"0x0003\" SASymSeqNum=\"0\" SASymRotation=\"0\" SASymError1=\"0x00\""
                                + " SASymError2=\"0x00\" SASymHideProgeny=\"0\" SASymHidden=\"0\" SASymOtherForm=\"0\" SASymHasDspMode=\"0\" SASymDspMode=\"0x0000\" SASymDspModeExt=\"0x00000000\" SASymCLevelNumber=\"0\""
                                + " SASymPenColorOn=\"1\" SASymPenColorRed=\"0\" SASymPenColorGreen=\"130\" SASymPenColorBlue=\"236\" SASymFillColorOn=\"1\" SASymFillColorRed=\"176\" SASymFillColorGreen=\"213\""
                                + " SASymFillColorBlue=\"255\" SASymFontColorOn=\"0\" SASymFontColorRed=\"0\" SASymFontColorGreen=\"0\" SASymFontColorBlue=\"0\" SASymLocX=\"" + loc_x + "\" SASymLocY=\"" + loc_y + "\" SASymSizeX=\"" + size_x + "\""
                                + " SASymSizeY=\"" + size_y + "\" SASymNameLocX=\"-150\" SASymNameLocY=\"-100\" SASymNameSizeX=\"0\" SASymNameSizeY=\"0\" SASymDescLocX=\"0\" SASymDescLocY=\"0\" SASymDescSizeX=\"0\" SASymDescSizeY=\"0\""
                                + " SASymZPPicFile=\"P" + count.ToString("D7") + ".BMP\" SASymZPPicType=\"0x0101\">");

                            writer.WriteRaw("<SAPicture SAPictureEncodingMethod=\"Hex\" SAPictureEncodingVersion=\"1.0\" SAOriginalFile=\"P" + count.ToString("D7") + ".BMP\" SAOriginalFileLength=\"152054\" SAPictureData=\"" + value.value + "\"/>"
                                + " <SAProperty SAPrpName=\"~C~\" SAPrpValue=\"2\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/><SAProperty SAPrpName=\"~T~\" SAPrpValue=\"11\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/></SASymbol>");
                        }

                        if (doc_block_views.TryGetValue(view.id, out value))
                        {
                            if (location_dic.TryGetValue(view.id, out location) == true)
                            {
                                loc_x = location.top_left_x;
                                loc_y = location.top_left_y;
                                size_x = (Convert.ToInt32(location.bottom_right_x) - Convert.ToInt32(location.top_left_x)).ToString();
                                size_y = (Convert.ToInt32(location.top_left_y) - Convert.ToInt32(location.bottom_right_y)).ToString();
                            }
                            else
                            {
                                loc_x = "574";
                                loc_y = "203";
                                size_x = "125";
                                size_y = "55";
                            }

                            writer.WriteRaw("<SASymbol SAObjId=\"" + value.id + "\" SAObjName=\"\" SAObjMinorTypeName=\"Doc Block\" SAObjMinorTypeNum=\"4\" SAObjMajorTypeNum=\"2\" SAObjAuditId=\"SAS\" SAObjUpdateDate=\"1/29/2015\" SAObjUpdateTime=\"3:01:32 PM\""
                            + " SAObjFQName=\"&quot;&quot;\" SASymIdDgm=\"" + view.id + "\" SASymArrangement=\"0\" SASymOtherSymbology=\"0\" SASymProperties=\"0x0000\" SASymOrder=\"0\" SASymXPEntity=\"3\" SASymXPLink=\"65535\" SASymXPGroup=\"65535\" SASymXPSibling=\"0\""
                            + " SASymXPSubordinate=\"65535\" SASymPenStyle=\"0x0010\" SASymFontName=\"\" SASymFontHeight=\"0x0000\" SASymFontFlags=\"0x0000\" SASymLineStyle=\"0x0103\" SASymFlags=\"0x0002\" SASymFlags2=\"0x0000\" SASymFlags3=\"0x0000\" SASymTextFlags=\"0x000a\""
                            + " SASymStyle=\"0\" SASymAuxStyle=\"0x0000\" SASymOccurs=\"0x01\" SASymOccOffset=\"0x00\" SASymBGColor=\"0x00\" SASymFGColor=\"0x00\" SASymPrompt=\"0x00\" SASymFrExArcChar=\"0x00\" SASymToExArcChar=\"0x00\" SASymUncleCount=\"0x00\""
                            + " SASymStyleFlags=\"0x0003\" SASymSeqNum=\"0\" SASymRotation=\"0\" SASymError1=\"0x00\" SASymError2=\"0x00\" SASymHideProgeny=\"0\" SASymHidden=\"0\" SASymOtherForm=\"0\" SASymHasDspMode=\"0\" SASymDspMode=\"0x0000\" SASymDspModeExt=\"0x00000000\""
                            + " SASymCLevelNumber=\"0\" SASymPenColorOn=\"1\" SASymPenColorRed=\"0\" SASymPenColorGreen=\"130\" SASymPenColorBlue=\"236\" SASymFillColorOn=\"1\" SASymFillColorRed=\"176\" SASymFillColorGreen=\"213\" SASymFillColorBlue=\"255\" SASymFontColorOn=\"0\""
                            + " SASymFontColorRed=\"0\" SASymFontColorGreen=\"0\" SASymFontColorBlue=\"0\" SASymLocX=\"" + loc_x + "\" SASymLocY=\"" + loc_y + "\" SASymSizeX=\"" + size_x + "\" SASymSizeY=\"" + size_y + "\" SASymNameLocX=\"569\" SASymNameLocY=\"166\" SASymNameSizeX=\"393\" SASymNameSizeY=\"51\""
                            + " SASymDescLocX=\"620\" SASymDescLocY=\"367\" SASymDescSizeX=\"273\" SASymDescSizeY=\"17\" SASymZPDesc=\"" + value.value + "\"><SAProperty SAPrpName=\"~C~\" SAPrpValue=\"2\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                            + "<SAProperty SAPrpName=\"~T~\" SAPrpValue=\"4\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/><SAProperty SAPrpName=\"Description\" SAPrpValue=\"" + value.value + "\" SAPrpEditType=\"1\" SAPrpLength=\"4074\"/></SASymbol>");
                        }

                        writer.WriteRaw(@"</SADiagram>");

                        foreach (Thing thing in thing_list)
                        {
                            if (!SA_Def_elements.Contains(thing.id))
                            {
                                SA_Def_elements.Add(thing.id);
                                thing_GUID = thing_GUIDs[thing.id];

                                minor_type = Find_Definition_Element_SA_Minor_Type(thing.type);

                                writer.WriteRaw("<SADefinition SAObjId=\"" + thing.id + "\" SAObjName=\"" + thing.name + "\" SAObjMinorTypeName=\"" + thing.type + "\" "
                                    + "SAObjMinorTypeNum=\"" + minor_type + "\" SAObjMajorTypeNum=\"3\" SAObjAuditId=\"NEAR\" SAObjUpdateDate=\"" + date + "\" "
                                    + "SAObjUpdateTime=\"" + time + "\" SAObjFQName=\"&quot;" + thing_GUID + "&quot;." + thing.name + "\">"
                                    + "<SAProperty SAPrpName=\"~C~\" SAPrpValue=\"3\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                    + "<SAProperty SAPrpName=\"~T~\" SAPrpValue=\"" + minor_type + "\" SAPrpEditType=\"0\" SAPrpLength=\"0\"/>"
                                    + "<SAProperty SAPrpName=\"GUID\" SAPrpValue=\"" + thing_GUID + "\" SAPrpEditType=\"1\" SAPrpLength=\"64\"/>"
                                    + "<SAProperty SAPrpName=\"KeyGUID\" SAPrpValue=\"" + thing_GUID + "\" SAPrpEditType=\"1\" SAPrpLength=\"80\"/>"
                                    + "<SAProperty SAPrpName=\"Is Instance\" SAPrpValue=\"F\" SAPrpEditType=\"4\" SAPrpLength=\"1\"/>"
                                    + ((minor_type == "1327") ? "" : "<SAProperty SAPrpName=\"To Line End\" SAPrpValue=\"LineEnd1\" SAPrpEditType=\"1\" SAPrpLength=\"1200\"/>"));

                                //

                                //<SAProperty SAPrpName="Parent Of Capability" SAPrpValue="Definition:&quot;Capability (DM2)&quot;:&quot;99be13e4-03b9-43f1-bf82-0d508bea5cc3&quot;.&quot;(JCA 1.0) Force Support&quot;
                                //    Definition:&quot;Capability (DM2)&quot;:a697273a-8c0e-4f84-b18e-7c6876dd0742.&quot;(JCA 2.0) Battlespace Awareness&quot;
                                //    Definition:&quot;Capability (DM2)&quot;:&quot;3f98f92e-fe73-43e6-b506-7e4e86f861db&quot;.&quot;(JCA 3.0) Force Application&quot;
                                //    Definition:&quot;Capability (DM2)&quot;:cd8cef3b-87a6-402f-9205-70e0794766c8.&quot;(JCA 4.0) Logistics&quot;
                                //    Definition:&quot;Capability (DM2)&quot;:c5376ccf-5b8f-4a10-9d94-ef39ae03b453.&quot;(JCA 5.0) Command and Control&quot;
                                //    Definition:&quot;Capability (DM2)&quot;:&quot;0f6cfd54-7aca-4c75-98b2-c7a785ad9fb6&quot;.&quot;(JCA 6.0) Net-Centric&quot;" SAPrpEditType="14" SAPrpLength="1200" SAPrpEditDefMajorType="Definition" SAPrpEditDefMinorType="Capability (DM2)">
                                //    <SALink SALinkName="&quot;(JCA 1.0) Force Support&quot;" SALinkIdentity="_15647"/>
                                //    <SALink SALinkName="&quot;(JCA 2.0) Battlespace Awareness&quot;" SALinkIdentity="_15639"/>
                                //    <SALink SALinkName="&quot;(JCA 3.0) Force Application&quot;" SALinkIdentity="_15644"/>
                                //    <SALink SALinkName="&quot;(JCA 4.0) Logistics&quot;" SALinkIdentity="_15648"/>
                                //    <SALink SALinkName="&quot;(JCA 5.0) Command and Control&quot;" SALinkIdentity="_15643"/>
                                //    <SALink SALinkName="&quot;(JCA 6.0) Net-Centric&quot;" SALinkIdentity="_15642"/>
                                //</SAProperty>

                                //

                                sorted_results = Get_Tuples_place1(thing, tuples);
                                sorted_results.AddRange(Get_Tuples_place1(thing, tuple_types));
                                sorted_results.AddRange(Get_Tuples_place2(thing, tuples));
                                sorted_results.AddRange(Get_Tuples_place2(thing, tuple_types));
                                values = new List<Thing>();
                                if(support_views.TryGetValue(view.id, out values))
                                    sorted_results.AddRange(Get_Tuples_place1(thing, values));
                                values = new List<Thing>();
                                if (needline_views.TryGetValue(view.id, out values))
                                    sorted_results.AddRange(Get_Tuples_id(thing, values));


                                if (sorted_results.Count() > 0)
                                {
                                    foreach (List<Thing> list in sorted_results)
                                    {
                                        count2 = 0;
                                        
                                        foreach (Thing rela in list)
                                        {
                                            
                                            if (thing_GUIDs.TryGetValue(rela.place2, out temp_GUID))
                                            {
                                                if (count2 == 0)
                                                {
                                                    temp = "<SAProperty SAPrpName=\"" + list.First().type + "\" SAPrpValue=\"";
                                                    temp3 = "";
                                                    temp2 = "";
                                                    count2++;
                                                }

                                                if (things.TryGetValue(rela.place2, out value))
                                                {
                                                    temp = temp + "Definition:&quot;" + value.value + "&quot;:&quot;" + temp_GUID + ".&quot;" + value.name + "&quot;";
                                                    temp2 = "\" SAPrpEditType=\"14\" SAPrpLength=\"1200\" SAPrpEditDefMajorType=\"Definition\" SAPrpEditDefMinorType=\"" + value.value + "\">";
                                                    temp3 = temp3 + "<SALink SALinkName=\"&quot;" + value.name + "&quot;\" SALinkIdentity=\"" + value.id + "\"/>";
                                                }
                                            }
                                        }

                                        if (count2 > 0)
                                            writer.WriteRaw(temp + temp2 + temp3 + "</SAProperty>");  
                                    }
                                }

                                //

                                writer.WriteRaw("<SAProperty SAPrpName=\"Initial Date\" SAPrpValue=\"" + prop_date + "\" SAPrpEditType=\"2\" SAPrpLength=\"10\"/>"
                               + "<SAProperty SAPrpName=\"Initial Time\" SAPrpValue=\"" + prop_time + "\" SAPrpEditType=\"7\" SAPrpLength=\"11\"/>"
                               + "<SAProperty SAPrpName=\"Initial Audit\" SAPrpValue=\"NEAR\" SAPrpEditType=\"1\" SAPrpLength=\"8\"/>"
                               + "<SAProperty SAPrpName=\"Last Change Date\" SAPrpValue=\"" + prop_date + "\" SAPrpEditType=\"2\" SAPrpLength=\"10\"/>"
                               + "<SAProperty SAPrpName=\"Last Change Time\" SAPrpValue=\"" + prop_time + "\" SAPrpEditType=\"7\" SAPrpLength=\"11\"/>"
                               + "<SAProperty SAPrpName=\"Last Change Audit\" SAPrpValue=\"NEAR\" SAPrpEditType=\"1\" SAPrpLength=\"8\"/>"
                               + "</SADefinition>");
                            }
                        }
                        //writer.WriteRaw(@"<MandatoryElements>");

                        //foreach (Thing thing in view.mandatory)
                        //{
                        //    writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>");
                        //}

                        //writer.WriteRaw(@"</MandatoryElements>");
                        //writer.WriteRaw(@"<OptionalElements>");

                        //foreach (Thing thing in view.optional)
                        //{
                        //    writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>");
                        //}

                        //writer.WriteRaw(@"</OptionalElements>");
                        //writer.WriteRaw("</" + view.type + ">");
                        writer.WriteRaw(@"</Class>");
                    }

                    //foreach (Thing thing in things)
                    //    writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id + "\" "
                    //        + (((string)thing.value == "$none$") ? "" : thing.value_type + "=\"" + (string)thing.value + "\"") + ">" + "<ideas:Name exemplarText=\"" + thing.name
                    //        + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">");

                    //foreach (Thing thing in tuple_types)
                    //    writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                    //    + "\" place1Type=\"id" + thing.place1 + "\" place2Type=\"id" + thing.place2 + "\"/>");

                    //foreach (Thing thing in tuples)
                    //    writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                    //    + "\" tuplePlace1=\"id" + thing.place1 + "\" tuplePlace2=\"id" + thing.place2 + "\"/>");

                    //writer.WriteRaw(@"</IdeasData>");

                    //writer.WriteRaw(@"<IdeasViews frameworkVersion=""DM2.02_Chg_1"" framework=""DoDAF"">");

                    writer.WriteRaw(@"</Classes>");

                    writer.Flush();
                }
                return sw.ToString();
            }
        }
        
        /////////RSA
        
        public static string RSA2PES(byte[] input)
        {
            IEnumerable<Location> locations = new List<Location>();
            IEnumerable<Thing> things = new List<Thing>();
            IEnumerable<Thing> tuple_types = new List<Thing>();
            IEnumerable<Thing> tuples = new List<Thing>();
            IEnumerable<Thing> results;
            IEnumerable<Location> results_loc;
            IEnumerable<Thing> view_elements = new List<Thing>();
            List<View> views = new List<View>();
            List<Thing> mandatory_list = new List<Thing>();
            List<Thing> optional_list = new List<Thing>();
            string temp;
           // Dictionary<string, List<Thing>> doc_blocks_data;
            Dictionary<string, List<Thing>> doc_blocks_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, Thing> OV1_pic_views = new Dictionary<string, Thing>();
            Dictionary<string, List<Thing>> needline_mandatory_views = new Dictionary<string, List<Thing>>();
            Dictionary<string, List<Thing>> needline_optional_views = new Dictionary<string, List<Thing>>();
            //Dictionary<string, List<Thing>> results_dic;
            XElement root = XElement.Load(new MemoryStream(input));
            List<List<Thing>> sorted_results = new List<List<Thing>>();
            List<List<Thing>> sorted_results_new = new List<List<Thing>>();
            bool representation_scheme = false;
            List<Thing> values = new List<Thing>();
            XNamespace ns = "http:///schemas/UPIA/_7hv4kEc6Ed-f1uPQXF_0HA/563";
            XNamespace ns2 = "http://www.omg.org/XMI";
            XNamespace ns3 = "http://schema.omg.org/spec/UML/2.2";
            Thing value;
            List<string> errors_list = new List<string>();

            //Regular Things

            foreach (string[] current_lookup in RSA_Element_Lookup)
            {

                results =
                    from result in root.Elements(ns + current_lookup[1])
                    //from result3 in root.Elements(ns + "View")
                    //from result4 in root.Descendants()
                    from result2 in root.Descendants()
                    //from result2 in root.Elements(ns3 + "Package").Elements("packagedElement")
                    //where result2.Attribute("name") != null
                    where (string)result.LastAttribute == (string)result2.Attribute(ns2 + "id")
                    //where (string)result3.LastAttribute == (string)result4.Attribute(ns2 + "id")
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = (string)result.LastAttribute,// + "///" +*/ (string)result.LastAttribute,//Attribute("base_Operation"),
                        name = (string)result2.Attribute("name"),//*/ (string)result.FirstAttribute,//Attribute(ns2 + "id"),
                        value = "$none$",
                        place1 = (string)result.Attribute(ns2 + "id"),
                        place2 = (string)result.LastAttribute,
                        foundation = current_lookup[2],
                        value_type = "$none$"
                    };

                things = things.Concat(results.ToList());
            }

            //SuperSubtupe

            results =
                from result in root.Descendants().Elements("generalization")
                //from result3 in root.Elements(ns + "View")
                //from result4 in root.Descendants()
                //from result2 in root.Descendants()
                //from result2 in root.Elements(ns3 + "Package").Elements("packagedElement")
                //where result2.Attribute("name") != null
                //where (string)result.Attribute == (string)result2.Attribute(ns2 + "id")
                //where (string)result3.LastAttribute == (string)result4.Attribute(ns2 + "id")
                select new Thing
                {
                    type = "superSubtype",
                    id = (string)result.Attribute("general") + (string)result.Parent.Attribute(ns2 + "id"),
                    name = "$none$",
                    value = "$none$",
                    place1 = (string)result.Attribute("general"),
                    place2 = (string)result.Parent.Attribute(ns2 + "id"),
                    foundation = "superSubtype",
                    value_type = "$none$"
                };

            tuples = tuples.Concat(results.ToList());

            //WholePartType

            results =
                from result in root.Descendants().Elements("ownedAttribute")
                //from result3 in root.Elements(ns + "View")
                //from result4 in root.Descendants()
                //from result2 in root.Descendants()
                //from result2 in root.Elements(ns3 + "Package").Elements("packagedElement")
                where (string)result.Attribute("aggregation") == "composite"
                //where (string)result.Attribute == (string)result2.Attribute(ns2 + "id")
                //where (string)result3.LastAttribute == (string)result4.Attribute(ns2 + "id")
                select new Thing
                {
                    type = "WholePartType",
                    id = (string)result.Attribute("type") + (string)result.Parent.Attribute(ns2 + "id"),
                    name = "$none$",
                    value = "$none$",
                    place1 = (string)result.Attribute("type"),
                    place2 = (string)result.Parent.Attribute(ns2 + "id"),
                    foundation = "WholePartType",
                    value_type = "$none$"
                };

            tuple_types = tuple_types.Concat(results.ToList());

            // OV-1 Pic

            OV1_pic_views =
                   (
                    from result in root.Descendants().Elements("styles")
                    where (string)result.Attribute("figureImageURI") != null

                    select new
                    {
                        key = ((string)result.Parent.Parent.Attribute(ns2 + "id")).Substring(2),
                        value = new Thing
                        {
                            type = "ArchitecturalDescription",
                            id = ((string)result.Attribute(ns2 + "id")).Substring(2),
                            name = (string)result.Attribute("figureImageURI"),
                            value = Encode((string)result.Attribute("figureImageURI")),
                            place1 = "$none$",
                            place2 = "$none$",
                            foundation = "IndividualType",
                            value_type = "Picture"
                        }
                    }).ToDictionary(a => a.key, a => a.value);

            
            //Diagramming

            foreach (Thing thing in things)
            {

                results_loc =
                    from result in root.Descendants().Elements("children")
                    //from result3 in root.Elements(ns + "View")
                    //from result4 in root.Descendants()
                    //from result2 in root.Descendants()
                    from result2 in result.Elements("layoutConstraint")
                    //where result2.Attribute("name") != null
                    where (string)result.Attribute("element") == thing.place2
                    //where (string)result3.LastAttribute == (string)result4.Attribute(ns2 + "id")
                    select new Location
                    {
                        id = (string)result.Attribute("element"),
                        top_left_x = (string)result2.Attribute("x"),
                        top_left_y = ((int)result2.Attribute("y") + ((result2.Attribute("height") == null) ? 1000 : (int)result2.Attribute("height"))).ToString(),
                        bottom_right_x = ((int)result2.Attribute("x") + ((result2.Attribute("width") == null) ? 1000 : (int)result2.Attribute("width"))).ToString(),
                        bottom_right_y = (string)result2.Attribute("y")

                    };

                locations = locations.Concat(results_loc.ToList());

            }

            foreach (Location location in locations)
            {
                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "Information",
                    id = location.id + "_12",
                    name = "Diagramming Information",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType"
                });

                values.Add(new Thing
                {
                    type = "Point",
                    id = location.id + "_16",
                    name = "Top Left Location",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType"
                });

                values.Add(new Thing
                {
                    type = "PointType",
                    id = location.id + "_14",
                    name = "Top Left LocationType",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType"
                });

                values.Add(new Thing
                {
                    type = "Point",
                    id = location.id + "_26",
                    name = "Bottome Right Location",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType"
                });

                values.Add(new Thing
                {
                    type = "PointType",
                    id = location.id + "_24",
                    name = "Bottome Right LocationType",
                    value = "$none$",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_18",
                    name = "Top Left X Location",
                    value = location.top_left_x,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_20",
                    name = "Top Left Y Location",
                    value = location.top_left_y,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_22",
                    name = "Top Left Z Location",
                    value = "0",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_28",
                    name = "Bottom Right X Location",
                    value = location.bottom_right_x,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_30",
                    name = "Bottom Right Y Location",
                    value = location.bottom_right_y,
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                values.Add(new Thing
                {
                    type = "SpatialMeasure",
                    id = location.id + "_32",
                    name = "Bottom Right Z Location",
                    value = "0",
                    place1 = "$none$",
                    place2 = "$none$",
                    foundation = "IndividualType",
                    value_type = "numericValue"
                });

                things = things.Concat(values);

                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "describedBy",
                    id = location.id + "_11",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id,
                    place2 = location.id + "_12",
                    foundation = "namedBy"
                });

                values.Add(new Thing
                {
                    type = "typeInstance",
                    id = location.id + "_15",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_14",
                    place2 = location.id + "_16",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "typeInstance",
                    id = location.id + "_25",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_24",
                    place2 = location.id + "_26",
                    foundation = "typeInstance"
                });


                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_17",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_18",
                    place2 = location.id + "_16",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_19",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_20",
                    place2 = location.id + "_16",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_21",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_22",
                    place2 = location.id + "_16",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_27",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_28",
                    place2 = location.id + "_26",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_29",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_30",
                    place2 = location.id + "_26",
                    foundation = "typeInstance"
                });

                values.Add(new Thing
                {
                    type = "measureOfIndividualPoint",
                    id = location.id + "_31",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_32",
                    place2 = location.id + "_26",
                    foundation = "typeInstance"
                });

                tuples = tuples.Concat(values);

                values = new List<Thing>();

                values.Add(new Thing
                {
                    type = "resourceInLocationType",
                    id = location.id + "_13",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_12",
                    place2 = location.id + "_14",
                    foundation = "CoupleType"
                });

                values.Add(new Thing
                {
                    type = "resourceInLocationType",
                    id = location.id + "_23",
                    name = "$none$",
                    value = "$none$",
                    place1 = location.id + "_12",
                    place2 = location.id + "_24",
                    foundation = "CoupleType"
                });

                tuple_types = tuple_types.Concat(values);
            }

            //Views

            foreach (string[] current_lookup in View_Lookup)
            {
                results =
                    from result in root.Descendants().Elements("contents").Elements("children")
                    //from result3 in root.Elements(ns + "View")
                    from result2 in root.Descendants()
                    //from result3 in root.Descendants()
                    //from result2 in root.Elements(ns3 + "Package").Elements("packagedElement")
                    //where result2.Attribute("name") != null
                    where (string)result2.LastAttribute == (string)result.Attribute("element")
                    where (string)result.Parent.Attribute("name") == current_lookup[0]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = (string)result.Parent.Attribute(ns2 + "id") + (string)result.Attribute("element"),// + "///" +*/ (string)result.LastAttribute,//Attribute("base_Operation"),
                        name = ((string)result.Parent.Attribute("name")).Replace("&", " And "),//*/ (string)result.FirstAttribute,//Attribute(ns2 + "id"),
                        value = Find_DM2_Type_RSA((string)result2.Name.LocalName.ToString()),
                        place1 = (string)result.Parent.Attribute(ns2 + "id"),
                        place2 = (string)result.Attribute("element"),
                        foundation = "$none$",
                        value_type = "element_type"
                    };

                sorted_results = results.GroupBy(x => x.name).Select(group => group.Distinct().ToList()).ToList();

                sorted_results_new = new List<List<Thing>>();
                Add_Tuples(ref sorted_results, ref sorted_results_new, tuples.ToList(), ref errors_list);
                Add_Tuples(ref sorted_results, ref sorted_results_new, tuple_types.ToList(), ref errors_list);
                sorted_results = sorted_results_new;

                foreach (List<Thing> view in sorted_results)
                {

                    mandatory_list = new List<Thing>();
                    optional_list = new List<Thing>();


                    foreach (Thing thing in view)
                    {
                        if (thing.place2 != null)
                        {
                            temp = Find_Mandatory_Optional((string)thing.value, view.First().name, thing.type, thing.place1, ref errors_list);
                            if (temp == "Mandatory")
                            {
                                mandatory_list.Add(new Thing { id = thing.place2, type = (string)thing.value });
                            }
                            if (temp == "Optional")
                            {
                                optional_list.Add(new Thing { id = thing.place2, type = (string)thing.value });
                            }

                            if (needline_mandatory_views.TryGetValue(thing.place2, out values))
                                mandatory_list.AddRange(values);

                            if (needline_optional_views.TryGetValue(thing.place2, out values))
                                optional_list.AddRange(values);
                        }
                    }

                    if (doc_blocks_views.TryGetValue(view.First().place1, out values))
                        optional_list.AddRange(values);

                    if (OV1_pic_views.TryGetValue(view.First().place1, out value))
                        mandatory_list.Add(value);

                    mandatory_list = mandatory_list.OrderBy(x => x.type).ToList();
                    optional_list = optional_list.OrderBy(x => x.type).ToList();

                    if (Proper_View(mandatory_list, view.First().name, view.First().type, view.First().place1, ref errors_list))
                        views.Add(new View { type = view.First().type, id = view.First().place1, name = view.First().name, mandatory = mandatory_list, optional = optional_list });
                }
            }

            using (var sw = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {

                    writer.WriteRaw(@"<IdeasEnvelope OriginatingNationISO3166TwoLetterCode=""String"" ism:ownerProducer=""NMTOKEN"" ism:classification=""U""
                    xsi:schemaLocation=""http://cio.defense.gov/xsd/dm2 DM2_PES_v2.02_Chg_1.XSD""
                    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ism=""urn:us:gov:ic:ism:v2"" xmlns:ideas=""http://www.ideasgroup.org/xsd""
                    xmlns:dm2=""http://www.ideasgroup.org/dm2""><IdeasData XMLTagsBoundToNamingScheme=""DM2Names"" ontologyVersion=""2.02_Chg_1"" ontology=""DM2"">
		            <NamingScheme ideas:FoundationCategory=""NamingScheme"" id=""ns1""><ideas:Name namingScheme=""ns1"" id=""NamingScheme"" exemplarText=""DM2Names""/>
		            </NamingScheme>");
                    if (representation_scheme)
                        writer.WriteRaw(@"<RepresentationScheme ideas:FoundationCategory=""Type"" id=""id_rs1"">
			            <ideas:Name id=""RepresentationScheme"" namingScheme=""ns1"" exemplarText=""Base64 Encoded Image""/>
		                </RepresentationScheme>");

                    foreach (Thing thing in things)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id + "\" "
                            + (((string)thing.value == "$none$") ? "" : thing.value_type + "=\"" + (string)thing.value + "\"") + ">" + "<ideas:Name exemplarText=\"" + thing.name
                            + "\" namingScheme=\"ns1\" id=\"n" + thing.id + "\"/></" + thing.type + ">");

                    foreach (Thing thing in tuple_types)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" place1Type=\"id" + thing.place1 + "\" place2Type=\"id" + thing.place2 + "\"/>");

                    foreach (Thing thing in tuples)
                        writer.WriteRaw("<" + thing.type + " ideas:FoundationCategory=\"" + thing.foundation + "\" id=\"id" + thing.id
                        + "\" tuplePlace1=\"id" + thing.place1 + "\" tuplePlace2=\"id" + thing.place2 + "\"/>");

                    writer.WriteRaw(@"</IdeasData>");

                    writer.WriteRaw(@"<IdeasViews frameworkVersion=""DM2.02_Chg_1"" framework=""DoDAF"">");

                    foreach (View view in views)
                    {
                        writer.WriteRaw("<" + view.type + " id=\"id" + view.id + "\" name=\"" + view.name + "\">");

                        writer.WriteRaw(@"<MandatoryElements>");

                        foreach (Thing thing in view.mandatory)
                        {
                            writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>");
                        }

                        writer.WriteRaw(@"</MandatoryElements>");
                        writer.WriteRaw(@"<OptionalElements>");

                        foreach (Thing thing in view.optional)
                        {
                            writer.WriteRaw("<" + view.type + "_" + thing.type + " ref=\"id" + thing.id + "\"/>");
                        }

                        writer.WriteRaw(@"</OptionalElements>");
                        writer.WriteRaw("</" + view.type + ">");
                    }

                    writer.WriteRaw(@"</IdeasViews>");

                    writer.WriteRaw(@"</IdeasEnvelope>");

                    writer.Flush();
                }
                return sw.ToString();
            }
        }
        
        ////////////////////
        ////////////////////
/*
        public static string PES2RSA(byte[] input)
        {
            Dictionary<string, Thing> things = new Dictionary<string, Thing>();
            Dictionary<string, Thing> results_dic;
            Dictionary<string, Thing> OV1_pic_views = new Dictionary<string, Thing>();
            IEnumerable<Thing> tuple_types = new List<Thing>();
            IEnumerable<Thing> tuples = new List<Thing>();
            IEnumerable<Thing> results;
            List<View> views = new List<View>();
            string temp="";
            string temp2="";
            string temp3="";
            string date = DateTime.Now.ToString("d");
            string time = DateTime.Now.ToString("T");
            string prop_date = DateTime.Now.ToString("yyyyMMdd");
            string prop_time = DateTime.Now.ToString("HHmmss");
            string minor_type;
            Guid view_GUID;
            string thing_GUID;
            string thing_GUID_1;
            string thing_GUID_2;
            string thing_GUID_3;
            Dictionary<string, string> thing_GUIDs = new Dictionary<string, string>();
            List<string> SA_Def_elements = new List<string>();
            XElement root = XElement.Load(new MemoryStream(input));
            List<List<Thing>> sorted_results;
            //bool representation_scheme = false;
            int count = 0;
            int count2 = 0;
            Thing value;
            //List<Thing> values;
            XNamespace ns = "http://www.ideasgroup.org/xsd";
            Dictionary<string, Location> location_dic = new Dictionary<string, Location>();
            string loc_x, loc_y, size_x, size_y;
            Location location;
            List<string> errors_list = new List<string>();

            foreach (string[] current_lookup in RSA_Element_Lookup)
            {
                results_dic =
                    (from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                        where (string)result.Parent.Name.ToString() == current_lookup[0]
                        select new
                        {
                            key = ((string)result.Parent.Attribute("id")).Substring(2),
                            value = new Thing
                            {
                                type = current_lookup[0],
                                id = ((string)result.Parent.Attribute("id")).Substring(2),
                                name = (string)result.Attribute("exemplarText"),
                                value = current_lookup[1],
                                place1 = "$none$",
                                place2 = "$none$",
                                foundation = (string)result.Parent.Attribute(ns + "FoundationCategory"),
                                value_type = "SAObjMinorTypeName"
                            }
                        }).ToDictionary(a => a.key, a => a.value);


                if (results_dic.Count() > 0)
                    MergeDictionaries(things, results_dic);
            }

            // OV-1 Pic

            OV1_pic_views =
                   (
                    from result2 in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                    where (string)result2.Name.ToString() == "OV-1_ArchitecturalDescription"
                    from result in root.Elements("IdeasData").Descendants().Elements(ns + "Name")
                    where ((string)result2.Attribute("ref")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                    from result3 in root.Elements("IdeasData").Elements("representationSchemeInstance")
                    //where (string)result.Parent.Name.ToString() == "ArchitecturalDescription"
                    where ((string)result3.Attribute("tuplePlace2")).Substring(2) == ((string)result.Parent.Attribute("id")).Substring(2)
                    select new
                    {
                        key = ((string)result2.Parent.Parent.Attribute("id")).Substring(2),
                        value = new Thing
                        {
                            type = "ArchitecturalDescription",
                            id = ((string)result.Parent.Attribute("id")).Substring(2),
                            name = (string)result.Attribute("exemplarText"),
                            value = ((string)result.Parent.Attribute("exemplar")),
                            place1 = "$none$",
                            place2 = "$none$",
                            foundation = (string)result.Parent.Attribute(ns + "FoundationCategory"),
                            value_type = "Picture"
                        }
                    }).ToDictionary(a => a.key, a => a.value);

            if (OV1_pic_views.Count() > 0)
            {
                foreach (Thing thing in OV1_pic_views.Values.ToList())
                {
                    things.Remove(thing.id);
                }
            }

            //  diagramming

            results =
                     from result in root.Elements("IdeasData").Elements("SpatialMeasure").Elements(ns + "Name")
                     select new Thing
                     {
                         id = ((string)result.Parent.Attribute("id")).Substring(2, ((string)result.Parent.Attribute("id")).Length - 5),
                         name = (string)result.Attribute("exemplarText"),
                         value = (string)result.Parent.Attribute("numericValue"),
                         place1 = "$none$",
                         place2 = "$none$",
                         foundation = "$none$",
                         value_type = "diagramming"
                     };

            sorted_results = results.GroupBy(x => x.id).Select(group => group.OrderBy(x => x.name).ToList()).ToList();

            foreach (List<Thing> coords in sorted_results)
            {
                location_dic.Add(coords.First().id,
                    new Location
                    {
                        id = coords.First().id,
                        bottom_right_x = (string)coords[0].value,
                        bottom_right_y = (string)coords[1].value,
                        bottom_right_z = "0",
                        top_left_x = (string)coords[3].value,
                        top_left_y = (string)coords[4].value,
                        top_left_z = "0",
                    });
            }

            // regular tuples

            foreach (string[] current_lookup in Tuple_Lookup)
            {
                if (current_lookup[3] != "1" && current_lookup[3] != "5")
                    continue;

                results =
                    from result in root.Elements("IdeasData").Descendants()
                    where (string)result.Name.ToString() == current_lookup[0]
                    from result2 in root.Elements("IdeasData").Descendants()
                    where ((string)result.Attribute("tuplePlace1")) == ((string)result2.Attribute("id"))
                    where (string)result2.Name.ToString() == current_lookup[5]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Attribute("id")).Substring(2),
                        name = "$none$",
                        value = (string)result2.Name.ToString(),
                        place1 = ((string)result.Attribute("tuplePlace1")).Substring(2),
                        place2 = ((string)result.Attribute("tuplePlace2")).Substring(2),
                        foundation = current_lookup[2],
                        value_type = "element type"
                    };

                tuples = tuples.Concat(results.ToList());
            }

            // regular tuple types

            foreach (string[] current_lookup in Tuple_Type_Lookup)
            {

                if (current_lookup[3] != "1" && current_lookup[3] != "5")
                    continue;

                results =
                    from result in root.Elements("IdeasData").Descendants()
                    where (string)result.Name.ToString() == current_lookup[0]
                    from result2 in root.Elements("IdeasData").Descendants()
                    where ((string)result.Attribute("place1Type")) == ((string)result2.Attribute("id"))
                    where (string)result2.Name.ToString() == current_lookup[5]

                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Attribute("id")).Substring(2),
                        name = "$none$",
                        value = (string)result2.Name.ToString(),
                        place1 = ((string)result.Attribute("place1Type")).Substring(2),
                        place2 = ((string)result.Attribute("place2Type")).Substring(2),
                        foundation = current_lookup[2],
                        value_type = "element type"
                    };

                tuple_types = tuple_types.Concat(results.ToList());
            }

            // views

            foreach (string[] current_lookup in View_Lookup)
            {
                if (current_lookup[3] != "default")
                    continue;
                results =
                    from result in root.Elements("IdeasViews").Descendants().Descendants().Descendants()
                    where (string)result.Parent.Parent.Name.ToString() == current_lookup[0]
                    select new Thing
                    {
                        type = current_lookup[0],
                        id = ((string)result.Parent.Parent.Attribute("id")).Substring(2) + ((string)result.Attribute("ref")).Substring(2),
                        name = ((string)result.Parent.Parent.Attribute("name")).Replace("&", " And "),
                        place1 = ((string)result.Parent.Parent.Attribute("id")).Substring(2),
                        place2 = ((string)result.Attribute("ref")).Substring(2),
                        value = (things.TryGetValue(((string)result.Attribute("ref")).Substring(2), out value)) ? value : new Thing { type = "$none$" },
                        foundation = "$none$",
                        value_type = "Thing"
                    };


                sorted_results = results.GroupBy(x => x.name).Select(group => group.Distinct().ToList()).ToList();
                //sorted_results = Add_Tuples(sorted_results, tuples);
                //sorted_results = Add_Tuples(sorted_results, tuple_types);

                foreach (List<Thing> view in sorted_results)
                {
                    List<Thing> mandatory_list = new List<Thing>();
                    List<Thing> optional_list = new List<Thing>();

                    foreach (Thing thing in view)
                    {

                        temp = Find_Mandatory_Optional((string)((Thing)thing.value).type, view.First().name, thing.type, thing.place1, ref errors_list);
                        if (temp == "Mandatory")
                        {
                            mandatory_list.Add(new Thing { id = thing.place2, name = (string)((Thing)thing.value).name, type = (string)((Thing)thing.value).value });
                        }
                        if (temp == "Optional")
                        {
                            optional_list.Add(new Thing { id = thing.place2, name = (string)((Thing)thing.value).name, type = (string)((Thing)thing.value).value });
                        }
                    }

                    mandatory_list = mandatory_list.OrderBy(x => x.type).ToList();
                    optional_list = optional_list.OrderBy(x => x.type).ToList();

                    //if (needline_views.TryGetValue(view.First().place1, out values))
                    //    optional_list.AddRange(values);

                    //if (Proper_View(mandatory_list, view.First().type))
                    views.Add(new View { type = current_lookup[1], id = view.First().place1, name = view.First().name, mandatory = mandatory_list, optional = optional_list });
                }
            }

            foreach (string thing in things.Keys)
            {
                thing_GUID = "_" + Guid.NewGuid().ToString("N").Substring(10);
                
                thing_GUID_3 = thing_GUID.Substring(7, 16);

                thing_GUIDs.Add(thing, thing_GUID_3);
            }

            //  output

            using (var sw = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {

                    writer.WriteRaw(@"<xmi:XMI xmi:version=""2.0"" xmlns:xmi=""http://www.omg.org/XMI"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:SoaML=""http:///schemas/SoaML/_LLF3UPc5EeGmUaPBBKwKBw/136"" xmlns:UPIA=""http:///schemas/UPIA/_7hv4kEc6Ed-f1uPQXF_0HA/563"" xmlns:ecore=""http://www.eclipse.org/emf/2002/Ecore"" xmlns:notation=""http://www.eclipse.org/gmf/runtime/1.0.2/notation"" xmlns:uml=""http://www.eclipse.org/uml2/3.0.0/UML"" xmlns:umlnotation=""http://www.ibm.com/xtools/1.5.3/Umlnotation"" xsi:schemaLocation=""http:///schemas/SoaML/_LLF3UPc5EeGmUaPBBKwKBw/136 pathmap://SOAML/SoaML.epx#_LLGeYPc5EeGmUaPBBKwKBw?SoaML/SoaML? http:///schemas/UPIA/_7hv4kEc6Ed-f1uPQXF_0HA/563 pathmap://UPIA_HOME/UPIA.epx#_7im0MEc6Ed-f1uPQXF_0HA?UPIA/UPIA?"">
                        <uml:Model xmi:id=""_9R-2X9PyEeSa1bJT-ij9YA"" name=""UPIA Model"" viewpoint="""">
                        <eAnnotations xmi:id=""_9R-2YNPyEeSa1bJT-ij9YA"" source=""uml2.diagrams""/>
                        <eAnnotations xmi:id=""_9R-2YdPyEeSa1bJT-ij9YA"" source=""com.ibm.xtools.common.ui.reduction.editingCapabilities"">
                          <details xmi:id=""_9R-2YtPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBFragment"" value=""1""/>
                          <details xmi:id=""_9R-2Y9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBArtifact"" value=""1""/>
                          <details xmi:id=""_9R-2ZNPyEeSa1bJT-ij9YA"" key=""updm.project.activity"" value=""1""/>
                          <details xmi:id=""_9R-2ZdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBFunction"" value=""1""/>
                          <details xmi:id=""_9R-2ZtPyEeSa1bJT-ij9YA"" key=""updm.standard.activity"" value=""1""/>
                          <details xmi:id=""_9R-2Z9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStructureDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2aNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSubsystem"" value=""1""/>
                          <details xmi:id=""_9R-2adPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBClass"" value=""1""/>
                          <details xmi:id=""_9R-2atPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBRelationship1"" value=""1""/>
                          <details xmi:id=""_9R-2a9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBRelationship2"" value=""1""/>
                          <details xmi:id=""_9R-2bNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStateMachine1"" value=""1""/>
                          <details xmi:id=""_9R-2bdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStateMachine2"" value=""1""/>
                          <details xmi:id=""_9R-2btPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComponent"" value=""1""/>
                          <details xmi:id=""_9R-2b9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBDeploymentSpecification"" value=""1""/>
                          <details xmi:id=""_9R-2cNPyEeSa1bJT-ij9YA"" key=""updm.strategic.activity"" value=""1""/>
                          <details xmi:id=""_9R-2cdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBAbstractionRelation"" value=""1""/>
                          <details xmi:id=""_9R-2ctPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBActivity1"" value=""1""/>
                          <details xmi:id=""_9R-2c9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBActivity2"" value=""1""/>
                          <details xmi:id=""_9R-2dNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBAction"" value=""1""/>
                          <details xmi:id=""_9R-2ddPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBActivityDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2dtPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBActivity3"" value=""1""/>
                          <details xmi:id=""_9R-2d9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBPackage"" value=""1""/>
                          <details xmi:id=""_9R-2eNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSequence1"" value=""1""/>
                          <details xmi:id=""_9R-2edPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSequence2"" value=""1""/>
                          <details xmi:id=""_9R-2etPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSequenceDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2e9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBDependancy"" value=""1""/>
                          <details xmi:id=""_9R-2fNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBLifeLine"" value=""1""/>
                          <details xmi:id=""_9R-2fdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBUsage"" value=""1""/>
                          <details xmi:id=""_9R-2ftPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBFreeFormDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2f9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInstance"" value=""1""/>
                          <details xmi:id=""_9R-2gNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComponentDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2gdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBEvent1"" value=""1""/>
                          <details xmi:id=""_9R-2gtPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBEvent2"" value=""1""/>
                          <details xmi:id=""_9R-2g9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTypes2"" value=""1""/>
                          <details xmi:id=""_9R-2hNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTypes4"" value=""1""/>
                          <details xmi:id=""_9R-2hdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCommunicationDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2htPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBConstraint"" value=""1""/>
                          <details xmi:id=""_9R-2h9PyEeSa1bJT-ij9YA"" key=""updm.organizational.activity"" value=""1""/>
                          <details xmi:id=""_9R-2iNPyEeSa1bJT-ij9YA"" key=""updm.performance.activity"" value=""1""/>
                          <details xmi:id=""_9R-2idPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInterface"" value=""1""/>
                          <details xmi:id=""_9R-2itPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInformationFlow"" value=""1""/>
                          <details xmi:id=""_9R-2i9PyEeSa1bJT-ij9YA"" key=""updm.system.activity"" value=""1""/>
                          <details xmi:id=""_9R-2jNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComment1"" value=""1""/>
                          <details xmi:id=""_9R-2jdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComment2"" value=""1""/>
                          <details xmi:id=""_9R-2jtPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCompositeStructure1"" value=""1""/>
                          <details xmi:id=""_9R-2j9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCollaboration"" value=""1""/>
                          <details xmi:id=""_9R-2kNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBRealization"" value=""1""/>
                          <details xmi:id=""_9R-2kdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCompositeStructure2"" value=""1""/>
                          <details xmi:id=""_9R-2ktPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStateChartDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2k9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBUseCase1"" value=""1""/>
                          <details xmi:id=""_9R-2lNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBClassDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2ldPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBUseCase2"" value=""1""/>
                          <details xmi:id=""_9R-2ltPyEeSa1bJT-ij9YA"" key=""updm.enterprise.activity"" value=""1""/>
                          <details xmi:id=""_9R-2l9PyEeSa1bJT-ij9YA"" key=""updm.service.activity"" value=""1""/>
                          <details xmi:id=""_9R-2mNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBUseCaseDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2mdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBDeployment1"" value=""1""/>
                          <details xmi:id=""_9R-2mtPyEeSa1bJT-ij9YA"" key=""updm.operational.activity"" value=""1""/>
                          <details xmi:id=""_9R-2m9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBDeployment2"" value=""1""/>
                          <details xmi:id=""_9R-2nNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBDeploymentDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2ndPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInteraction"" value=""1""/>
                          <details xmi:id=""_9R-2ntPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCommunication"" value=""1""/>
                          <details xmi:id=""_9R-2n9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.mq"" value=""1""/>
                          <details xmi:id=""_9R-2oNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.ldap"" value=""1""/>
                          <details xmi:id=""_9R-2odPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.j2ee"" value=""1""/>
                          <details xmi:id=""_9R-2otPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBPrimitiveTypeTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-2o9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.analysisAndDesign.zephyrUML"" value=""1""/>
                          <details xmi:id=""_9R-2pNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBElementImport1"" value=""1""/>
                          <details xmi:id=""_9R-2pdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBElementImport2"" value=""1""/>
                          <details xmi:id=""_9R-2ptPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.db2"" value=""1""/>
                          <details xmi:id=""_9R-2p9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.mq"" value=""1""/>
                          <details xmi:id=""_9R-2qNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInterfaceTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-2qdPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.was"" value=""1""/>
                          <details xmi:id=""_9R-2qtPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.ldap"" value=""1""/>
                          <details xmi:id=""_9R-2q9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.dotnet"" value=""1""/>
                          <details xmi:id=""_9R-2rNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.j2ee"" value=""1""/>
                          <details xmi:id=""_9R-2rdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBProfile"" value=""1""/>
                          <details xmi:id=""_9R-2rtPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.storage"" value=""1""/>
                          <details xmi:id=""_9R-2r9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.waswebplugin"" value=""1""/>
                          <details xmi:id=""_9R-2sNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.dotnet"" value=""1""/>
                          <details xmi:id=""_9R-2sdPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.portlet"" value=""1""/>
                          <details xmi:id=""_9R-2stPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSignal"" value=""1""/>
                          <details xmi:id=""_9R-2s9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.waswebplugin"" value=""1""/>
                          <details xmi:id=""_9R-2tNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.messaging"" value=""1""/>
                          <details xmi:id=""_9R-2tdPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.jms"" value=""1""/>
                          <details xmi:id=""_9R-2ttPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.sqlserver"" value=""1""/>
                          <details xmi:id=""_9R-2t9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBActivity4"" value=""1""/>
                          <details xmi:id=""_9R-2uNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBClassTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-2udPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTemplate"" value=""1""/>
                          <details xmi:id=""_9R-2utPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.net"" value=""1""/>
                          <details xmi:id=""_9R-2u9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.messagebroker"" value=""1""/>
                          <details xmi:id=""_9R-2vNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSpecificInstanceType1"" value=""1""/>
                          <details xmi:id=""_9R-2vdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.viz.webservice.capabilty"" value=""1""/>
                          <details xmi:id=""_9R-2vtPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBSpecificInstanceType2"" value=""1""/>
                          <details xmi:id=""_9R-2v9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBObjectDiagram"" value=""1""/>
                          <details xmi:id=""_9R-2wNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.os"" value=""1""/>
                          <details xmi:id=""_9R-2wdPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.systemp"" value=""1""/>
                          <details xmi:id=""_9R-2wtPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.derby"" value=""1""/>
                          <details xmi:id=""_9R-2w9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.os"" value=""1""/>
                          <details xmi:id=""_9R-2xNPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStereotypedArtifact"" value=""1""/>
                          <details xmi:id=""_9R-2xdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTypes1"" value=""1""/>
                          <details xmi:id=""_9R-2xtPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.systemz"" value=""1""/>
                          <details xmi:id=""_9R-2x9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTypes3"" value=""1""/>
                          <details xmi:id=""_9R-2yNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.virtualization"" value=""1""/>
                          <details xmi:id=""_9R-2ydPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.ihs"" value=""1""/>
                          <details xmi:id=""_9R-2ytPyEeSa1bJT-ij9YA"" key=""com.ibm.ccl.soa.deploy.core.ui.activity.core"" value=""1""/>
                          <details xmi:id=""_9R-2y9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBPackageTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-2zNPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.virtualization"" value=""1""/>
                          <details xmi:id=""_9R-2zdPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComment3"" value=""1""/>
                          <details xmi:id=""_9R-2ztPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.modeling.enterprise.services.uireduction.activity"" value=""1""/>
                          <details xmi:id=""_9R-2z9PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.iis"" value=""1""/>
                          <details xmi:id=""_9R-20NPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBFunctionTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-20dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.was"" value=""1""/>
                          <details xmi:id=""_9R-20tPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.rest.ui.uireduction.activity"" value=""1""/>
                          <details xmi:id=""_9R-209PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.operation"" value=""1""/>
                          <details xmi:id=""_9R-21NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.portlet"" value=""1""/>
                          <details xmi:id=""_9R-21dPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInteractionOverview"" value=""1""/>
                          <details xmi:id=""_9R-21tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.db2"" value=""1""/>
                          <details xmi:id=""_9R-219PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.messaging"" value=""1""/>
                          <details xmi:id=""_9R-22NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.storage"" value=""1""/>
                          <details xmi:id=""_9R-22dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.server"" value=""1""/>
                          <details xmi:id=""_9R-22tPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBInteractionOverviewDiagram"" value=""1""/>
                          <details xmi:id=""_9R-229PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.tomcat"" value=""1""/>
                          <details xmi:id=""_9R-23NPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBRelationship3"" value=""1""/>
                          <details xmi:id=""_9R-23dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.server"" value=""1""/>
                          <details xmi:id=""_9R-23tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.tomcat"" value=""1""/>
                          <details xmi:id=""_9R-239PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.javaVisualizerActivity"" value=""1""/>
                          <details xmi:id=""_9R-24NPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStereotypedDeployment1"" value=""1""/>
                          <details xmi:id=""_9R-24dPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.transform.uml2.xsd.profile.uireduction.activity"" value=""1""/>
                          <details xmi:id=""_9R-24tPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.modeling.soa.ml.uireduction.activity"" value=""1""/>
                          <details xmi:id=""_9R-249PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.jms"" value=""1""/>
                          <details xmi:id=""_9R-25NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.sqlserver"" value=""1""/>
                          <details xmi:id=""_9R-25dPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.http.activity.id"" value=""1""/>
                          <details xmi:id=""_9R-25tPyEeSa1bJT-ij9YA"" key=""com.ibm.ccl.soa.deploy.core.ui.activity.generic"" value=""1""/>
                          <details xmi:id=""_9R-259PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.systemp"" value=""1""/>
                          <details xmi:id=""_9R-26NPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBCollaborationUse"" value=""1""/>
                          <details xmi:id=""_9R-26dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.derby"" value=""1""/>
                          <details xmi:id=""_9R-26tPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTiming"" value=""1""/>
                          <details xmi:id=""_9R-269PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.net"" value=""1""/>
                          <details xmi:id=""_9R-27NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.systemz"" value=""1""/>
                          <details xmi:id=""_9R-27dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.messagebroker"" value=""1""/>
                          <details xmi:id=""_9R-27tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.portal"" value=""1""/>
                          <details xmi:id=""_9R-279PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBComponentTemplateParameter"" value=""1""/>
                          <details xmi:id=""_9R-28NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.ihs"" value=""1""/>
                          <details xmi:id=""_9R-28dPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBTimingDiagram"" value=""1""/>
                          <details xmi:id=""_9R-28tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.portal"" value=""1""/>
                          <details xmi:id=""_9R-289PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.oracle"" value=""1""/>
                          <details xmi:id=""_9R-29NPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStereotypedClass"" value=""1""/>
                          <details xmi:id=""_9R-29dPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBProfileApplication"" value=""1""/>
                          <details xmi:id=""_9R-29tPyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.umlBBStereotypedComponent"" value=""1""/>
                          <details xmi:id=""_9R-299PyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.db2z"" value=""1""/>
                          <details xmi:id=""_9R-2-NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.database"" value=""1""/>
                          <details xmi:id=""_9R-2-dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.infrastructure.http"" value=""1""/>
                          <details xmi:id=""_9R-2-tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.oracle"" value=""1""/>
                          <details xmi:id=""_9R-2-9PyEeSa1bJT-ij9YA"" key=""com.ibm.xtools.activities.analysisAndDesign.zephyrAnalysis"" value=""1""/>
                          <details xmi:id=""_9R-2_NPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.iis"" value=""1""/>
                          <details xmi:id=""_9R-2_dPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.db2z"" value=""1""/>
                          <details xmi:id=""_9R-2_tPyEeSa1bJT-ij9YA"" key=""com.ibm.rational.deployment.activity.physical.http"" value=""1""/>
                        </eAnnotations>
                        <eAnnotations xmi:id=""_9R-2_9PyEeSa1bJT-ij9YA"" source=""com.ibm.xtools.updm.migration.marker"">
                          <details xmi:id=""_9R-3ANPyEeSa1bJT-ij9YA"" key=""SourceVersion"" value=""UPIA v1.2""/>
                          <details xmi:id=""_9R-3AdPyEeSa1bJT-ij9YA"" key=""TargetVersion"" value=""UPIA v1.3""/>
                          <details xmi:id=""_9R-3AtPyEeSa1bJT-ij9YA"" key=""UserNotified"" value=""1""/>
                        </eAnnotations>
                        <eAnnotations xmi:id=""_9R-3A9PyEeSa1bJT-ij9YA"" source=""com.ibm.xtools.upia.soaml.integration"">
                          <details xmi:id=""_9R-3BNPyEeSa1bJT-ij9YA"" key=""state"" value=""SoaMLApplied""/>
                        </eAnnotations>
                        <packageImport xmi:id=""_9R-3BdPyEeSa1bJT-ij9YA"">
                          <importedPackage xmi:type=""uml:Model"" href=""pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml#_0""/>
                        </packageImport>
                        <packageImport xmi:id=""_9R-3BtPyEeSa1bJT-ij9YA"">
                          <importedPackage xmi:type=""uml:Model"" href=""pathmap://UPIA_HOME/UPIAModelLibrary.emx#_1ayqoFrNEduq4eXjMcjG2g?UPIAModelLibrary?""/>
                        </packageImport>
                        <packagedElement xmi:type=""uml:Package"" xmi:id=""_9R-3B9PyEeSa1bJT-ij9YA"" name=""UPIA Model Architecture Description"">
                          <eAnnotations xmi:id=""_9R-3CNPyEeSa1bJT-ij9YA"" source=""uml2.diagrams""/>
                        </packagedElement>
                        <packagedElement xmi:type=""uml:Package"" xmi:id=""_GJaJsNPzEeSa1bJT-ij9YA"" name=""PES Data"">
                        <eAnnotations xmi:id=""_GSyIINPzEeSa1bJT-ij9YA"" source=""uml2.diagrams"" references=""_GSyIIdPzEeSa1bJT-ij9YA"">");

                    foreach (View view in views)
                    {
                        List<Thing> thing_list = new List<Thing>(view.mandatory);
                        thing_list.AddRange(view.optional);

                        writer.WriteRaw("<contents xmi:type=\"umlnotation:UMLDiagram\" xmi:id=\"" + view.id + "\" type=\"Freeform\" name=\"" + view.name + "\">");

                        if (OV1_pic_views.TryGetValue(view.id, out value))
                        {
                            thing_GUID = "_" + Guid.NewGuid().ToString("N").Substring(10);

                            thing_GUID_1 = "_02";

                            thing_GUID_3 = thing_GUID.Substring(7, 16);

                            writer.WriteRaw("<children xmi:id=\"" + thing_GUID_1 + "1111" + thing_GUID_3 + "\" type=\"skpicture\">");
                            writer.WriteRaw("<children xmi:id=\"" + thing_GUID_1 + "2222" + thing_GUID_3 + "\" type=\"skshapes\">");
                            writer.WriteRaw("<styles xmi:type=\"notation:DrawerStyle\" xmi:id=\"" + thing_GUID_1 + "3333" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:TitleStyle\" xmi:id=\"" + thing_GUID_1 + "4444" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:DrawerStyle\" xmi:id=\"" + thing_GUID_1 + "5555" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:TitleStyle\" xmi:id=\"" + thing_GUID_1 + "6666" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<element xsi:nil=\"true\"/>");
                            writer.WriteRaw("</children>");
                            writer.WriteRaw("<children xmi:id=\"" + thing_GUID_1 + "7777" + thing_GUID_3 + "\" type=\"skdescription\">");
                            writer.WriteRaw("<element xsi:nil=\"true\"/>");
                            writer.WriteRaw("</children>");
                            writer.WriteRaw("<styles xmi:type=\"notation:ShapeStyle\" xmi:id=\"" + thing_GUID_1 + "8888" + thing_GUID_3 + "\" description=\"picture\" transparency=\"0\" lineWidth=\"3\" roundedBendpointsRadius=\"12\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:LineTypeStyle\" xmi:id=\"" + thing_GUID_1 + "9999" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"SketchNotation:SketchStyle\" xmi:id=\"" + thing_GUID_1 + "aaaa" + thing_GUID_3 + "\" figureOverride=\"1\" figureImageURI=\"" + value.name + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:RoundedCornersStyle\" xmi:id=\"" + thing_GUID_1 + "bbbb" + thing_GUID_3 + "\"/>");
                            writer.WriteRaw("<styles xmi:type=\"notation:TextStyle\" xmi:id=\"" + thing_GUID_1 + "cccc" + thing_GUID_3 + "\" textAlignment=\"Center\"/>");
                            writer.WriteRaw("<element xsi:nil=\"true\"/>");
                            writer.WriteRaw("<layoutConstraint xmi:type=\"notation:Bounds\" xmi:id=\"" + thing_GUID_1 + "dddd" + thing_GUID_3 + "\" x=\"5706\" y=\"3804\"/>");
                            writer.WriteRaw("</children>");
                        }

                        foreach (Thing thing in thing_list)
                        {

                            thing_GUID_1 = "_00";

                            thing_GUID_3 = thing_GUIDs[thing.id];

                            if (location_dic.TryGetValue(thing.id, out location) == true)
                            {
                                loc_x = location.top_left_x;
                                loc_y = location.top_left_y;
                                size_x = (Convert.ToInt32(location.bottom_right_x) - Convert.ToInt32(location.top_left_x)).ToString();
                                size_y = (Convert.ToInt32(location.top_left_y) - Convert.ToInt32(location.bottom_right_y)).ToString();
                            }
                            else
                            {
                                loc_x = "$none$";
                                loc_y = "$none$";
                                size_x = "$none$";
                                size_y = "$none$";
                            }

                            writer.WriteRaw("<children xmi:type=\"umlnotation:UMLShape\" xmi:id=\"" + thing_GUID_1 + "1111" + thing_GUID_3 + "\" element=\"" + "_AAZZZZ" + thing_GUID_3 + "\" fontHeight=\"8\" transparency=\"0\" lineColor=\"14263149\" lineWidth=\"1\" showStereotype=\"Label\">");
                            writer.WriteRaw("<children xmi:type=\"notation:DecorationNode\" xmi:id=\"" + thing_GUID_1 + "2222" + thing_GUID_3 + "\" type=\"ImageCompartment\">");
                            writer.WriteRaw("<layoutConstraint xmi:type=\"notation:Size\" xmi:id=\"" + thing_GUID_1 + "3333" + thing_GUID_3 + "\" width=\"1320\" height=\"1320\"/></children>");
                            writer.WriteRaw("<children xmi:type=\"notation:BasicDecorationNode\" xmi:id=\"" + thing_GUID_1 + "4444" + thing_GUID_3 + "\" type=\"Stereotype\"/>");
                            writer.WriteRaw("<children xmi:type=\"notation:BasicDecorationNode\" xmi:id=\"" + thing_GUID_1 + "5555" + thing_GUID_3 + "\" type=\"Name\"/>");
                            writer.WriteRaw("<children xmi:type=\"notation:BasicDecorationNode\" xmi:id=\"" + thing_GUID_1 + "6666" + thing_GUID_3 + "\" type=\"Parent\"/>");
                            writer.WriteRaw("<children xmi:type=\"notation:SemanticListCompartment\" xmi:id=\"" + thing_GUID_1 + "7777" + thing_GUID_3 + "\" type=\"AttributeCompartment\"/>");
                            writer.WriteRaw("<children xmi:type=\"notation:SemanticListCompartment\" xmi:id=\"" + thing_GUID_1 + "8888" + thing_GUID_3 + "\" type=\"OperationCompartment\"/>");
                            writer.WriteRaw("<children xmi:type=\"notation:SemanticListCompartment\" xmi:id=\"" + thing_GUID_1 + "9999" + thing_GUID_3 + "\" visible=\"false\" type=\"SignalCompartment\"/>");
                            writer.WriteRaw("<children xmi:type=\"umlnotation:UMLShapeCompartment\" xmi:id=\"" + thing_GUID_1 + "aaaa" + thing_GUID_3 + "\" visible=\"false\" type=\"StructureCompartment\"/>");
                            writer.WriteRaw("<layoutConstraint xmi:type=\"notation:Bounds\" xmi:id=\"" + thing_GUID_1 + "bbbb" + thing_GUID_3 + "\""
                            + ((loc_x == "$none$") ? "" : " x=\"" + loc_x + "\"")
                            + ((loc_y == "$none$") ? "" : " y=\"" + loc_y + "\"")
                            + ((size_x == "$none$") ? "" : " width=\"" + size_x + "\"")
                            + ((size_y == "$none$") ? "" : " height=\"" + size_y + "\"")
                            + "/></children>");
                        }

                        writer.WriteRaw(@"<element xsi:nil=""true""/>");

                        foreach (Thing thing in thing_list)
                        {

                            thing_GUID_1 = "_00";

                            sorted_results = Get_Tuples_place1(thing, tuples);

                            foreach (List<Thing> values in sorted_results)
                            {
                                thing_GUID_2 = thing_GUIDs[values[0].place1];
                                thing_GUID_3 = thing_GUIDs[values[0].place2];

                                writer.WriteRaw("<edges xmi:type=\"umlnotation:UMLConnector\" xmi:id=\"" + thing_GUID_1 + "cccc" + thing_GUID_2 + "\" element=\"" + thing_GUID_1 + "dddd" + thing_GUID_2 + "\" source=\"" + thing_GUID_1 + "1111" + thing_GUID_2 + "\" target=\"" + thing_GUID_1 + "1111" + thing_GUID_3 + "\" fontHeight=\"8\" roundedBendpointsRadius=\"4\" routing=\"Rectilinear\" lineColor=\"8421504\" lineWidth=\"1\" showStereotype=\"Text\">");
                                writer.WriteRaw("<children xmi:type=\"notation:DecorationNode\" xmi:id=\"" + thing_GUID_1 + "eeee" + thing_GUID_2 + "\" type=\"NameLabel\">");
                                writer.WriteRaw("<children xmi:type=\"notation:BasicDecorationNode\" xmi:id=\"" + thing_GUID_1 + "ffff" + thing_GUID_2 + "\" type=\"Stereotype\"/>");
                                writer.WriteRaw("<children xmi:type=\"notation:BasicDecorationNode\" xmi:id=\"" + thing_GUID_1 + "gggg" + thing_GUID_2 + "\" type=\"Name\"/>");
                                writer.WriteRaw("<layoutConstraint xmi:type=\"notation:Bounds\" xmi:id=\"" + thing_GUID_1 + "hhhh" + thing_GUID_2 + "\" y=\"-186\"/>");
                                writer.WriteRaw("</children>");
                                writer.WriteRaw("<bendpoints xmi:type=\"notation:RelativeBendpoints\" xmi:id=\"" + thing_GUID_1 + "iiii" + thing_GUID_2 + "\" points=\"[6, 42, 32, -157]$[29, 168, 55, -31]\"/>");
                                writer.WriteRaw("</edges>");
                            }
                        }

                        writer.WriteRaw(@"</contents>
                                    </eAnnotations>");
                    }

                    foreach (KeyValuePair<string, Thing> thing in things)
                    {

                        //if (thing_GUIDs.TryGetValue(thing.Value.id, out thing_GUID) == false)
                        //{

                        //    thing_GUID = "_" + Guid.NewGuid().ToString("N").Substring(10);
                        //    thing_GUID_1 = thing_GUID.Substring(0, 3);
                        //    thing_GUID_3 = thing_GUID.Substring(7, 16);

                        //    thing_GUIDs.Add(thing.Value.id, thing_GUID_3);

                        //}

                         sorted_results = Get_Tuples_place1(thing.Value, tuples);
                         count = sorted_results.Count();
                         sorted_results.AddRange(Get_Tuples_place1(thing.Value, tuple_types));
                         count2 = sorted_results.Count();

                         if (count != 0)
                             foreach (List<Thing> values in sorted_results)
                             {
                                 thing_GUID_2 = thing_GUIDs[values[0].place1];
                                 thing_GUID_3 = thing_GUIDs[values[0].place2];

                                 writer.WriteRaw("<packagedElement xmi:type=\"uml:Class\" xmi:id=\"" + "_AAZZZZ" + thing_GUID_2 + "\" name=\"" + thing.Value.name + "\">");
                                 writer.WriteRaw("<generalization xmi:id=\"_VF0JMPAvEeSRVK9XlySZNA\" general=\"_I86dsPAvEeSRVK9XlySZNA\"/>");
                                 writer.WriteRaw("</packagedElement>");
                             }
                         else if (count2 == 1 + count)
                             foreach (List<Thing> values in sorted_results)
                             {
                                 thing_GUID_2 = thing_GUIDs[values[0].place1];
                                 thing_GUID_3 = thing_GUIDs[values[0].place2];

                                 writer.WriteRaw("<packagedElement xmi:type=\"uml:Class\" xmi:id=\"" + "_AAZZZZ" + thing_GUID_2 + "\" name=\"" + thing.Value.name + "\">");
                                 writer.WriteRaw("<ownedAttribute xmi:id=\"_cui1IPAvEeSRVK9XlySZNA\" name=\"activity 2\" visibility=\"private\" type=\"_I86dsPAvEeSRVK9XlySZNA\" aggregation=\"composite\" association=\"_cuZEIPAvEeSRVK9XlySZNA\">");
                                 writer.WriteRaw("<upperValue xmi:type=\"uml:LiteralUnlimitedNatural\" xmi:id=\"_cui1IvAvEeSRVK9XlySZNA\" value=\"*\"/>");
                                 writer.WriteRaw("<lowerValue xmi:type=\"uml:LiteralInteger\" xmi:id=\"_cui1IfAvEeSRVK9XlySZNA\"/>");
                                 writer.WriteRaw("</ownedAttribute>");
                                 writer.WriteRaw("</packagedElement>");
                             }
                         else
                         {
                             thing_GUID = thing_GUIDs[thing.Value.id];

                             writer.WriteRaw("<packagedElement xmi:type=\"uml:Class\" xmi:id=\"" + "_AAZZZZ" + thing_GUID + "\" name=\"" + thing.Value.name + "\"/>");
                         }
                    }

                    writer.WriteRaw(@"</packagedElement><profileApplication xmi:id=""_9R-3CdPyEeSa1bJT-ij9YA"">
                          <eAnnotations xmi:id=""_9R-3CtPyEeSa1bJT-ij9YA"" source=""http://www.eclipse.org/uml2/2.0.0/UML"">
                            <references xmi:type=""ecore:EPackage"" href=""pathmap://UML_PROFILES/Standard.profile.uml#_yzU58YinEdqtvbnfB2L_5w""/>
                          </eAnnotations>
                          <appliedProfile href=""pathmap://UML_PROFILES/Standard.profile.uml#_0""/>
                        </profileApplication>
                        <profileApplication xmi:id=""_9R-3C9PyEeSa1bJT-ij9YA"">
                          <eAnnotations xmi:id=""_9R-3DNPyEeSa1bJT-ij9YA"" source=""http://www.eclipse.org/uml2/2.0.0/UML"">
                            <references xmi:type=""ecore:EPackage"" href=""pathmap://UML2_MSL_PROFILES/Default.epx#_fNwoAAqoEd6-N_NOT9vsCA?Default/Default?""/>
                          </eAnnotations>
                          <appliedProfile href=""pathmap://UML2_MSL_PROFILES/Default.epx#_a_S3wNWLEdiy4IqP8whjFA?Default?""/>
                        </profileApplication>
                        <profileApplication xmi:id=""_9R-3DdPyEeSa1bJT-ij9YA"">
                          <eAnnotations xmi:id=""_9R-3DtPyEeSa1bJT-ij9YA"" source=""http://www.eclipse.org/uml2/2.0.0/UML"">
                            <references xmi:type=""ecore:EPackage"" href=""pathmap://UML2_MSL_PROFILES/Deployment.epx#_IrdAUMmBEdqBcN1R6EvWUw?Deployment/Deployment?""/>
                          </eAnnotations>
                          <appliedProfile href=""pathmap://UML2_MSL_PROFILES/Deployment.epx#_vjbuwOvHEdiDX5bji0iVSA?Deployment?""/>
                        </profileApplication>
                        <profileApplication xmi:id=""_9R-3D9PyEeSa1bJT-ij9YA"">
                          <eAnnotations xmi:id=""_9R-3ENPyEeSa1bJT-ij9YA"" source=""http://www.eclipse.org/uml2/2.0.0/UML"">
                            <references xmi:type=""ecore:EPackage"" href=""pathmap://UPIA_HOME/UPIA.epx#_7im0MEc6Ed-f1uPQXF_0HA?UPIA/UPIA?""/>
                          </eAnnotations>
                          <appliedProfile href=""pathmap://UPIA_HOME/UPIA.epx#_c2-k4GUFEduIxJjDZy3KpA?UPIA?""/>
                        </profileApplication>
                        <profileApplication xmi:id=""_9R-3EdPyEeSa1bJT-ij9YA"">
                          <eAnnotations xmi:id=""_9R-3EtPyEeSa1bJT-ij9YA"" source=""http://www.eclipse.org/uml2/2.0.0/UML"">
                            <references xmi:type=""ecore:EPackage"" href=""pathmap://SOAML/SoaML.epx#_LLGeYPc5EeGmUaPBBKwKBw?SoaML/SoaML?""/>
                          </eAnnotations>
                          <appliedProfile href=""pathmap://SOAML/SoaML.epx#_ut1IIGfDEdy6JoIZoRRqYw?SoaML?""/>
                        </profileApplication>
                      </uml:Model>
                      <UPIA:EnterpriseModel xmi:id=""_9R-3E9PyEeSa1bJT-ij9YA"" base_Package=""_9R-2X9PyEeSa1bJT-ij9YA""/>
                      <UPIA:ArchitectureDescription xmi:id=""_9R-3FNPyEeSa1bJT-ij9YA"" base_Package=""_9R-3B9PyEeSa1bJT-ij9YA""/>
                      <UPIA:View xmi:id=""_GSChQNPzEeSa1bJT-ij9YA"" base_Package=""_GJaJsNPzEeSa1bJT-ij9YA""/>");

                    foreach (KeyValuePair<string, Thing> thing in things)
                    {

                        thing_GUID = thing_GUIDs[thing.Value.id];

                        writer.WriteRaw("<UPIA:System xmi:id=\"" + "_BBZZZZ" + thing_GUID + "\" base_Class=\"" + "_AAZZZZ" + thing_GUID + "\"/>");
                    }

                    writer.WriteRaw(@"</xmi:XMI>");

                    writer.Flush();
                }
                return sw.ToString();
            }
        }*/
    }
       
}
