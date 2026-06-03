using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Infrastructure.Persistence.Seeding.Data;

/// <summary>
/// Provides predefined clinical form templates for database seeding.
/// </summary>
public static class ClinicalFormTemplateData
{
    private const string vitalSigns = """
        "vitalSigns": {
          "type": "object",
          "required": ["bloodPressure", "heartRate", "temperature", "weight", "height"],
          "properties": {
            "bloodPressure": { "type": "string" },
            "heartRate": { "type": "integer" },
            "respiratoryRate": { "type": "integer" },
            "temperature": { "type": "number" },
            "oxygenSaturation": { "type": "number" },
            "weight": { "type": "number" },
            "height": { "type": "number" },
            "bmi": { "type": "number" }
          }
        }
        """;

    private const string anamnesis = """
        "anamnesis": {
          "type": "object",
          "properties": {
            "evolutionTime": { "type": "string" },
            "characteristics": { "type": "string" },
            "aggravatingFactors": { "type": "string" },
            "relievingFactors": { "type": "string" },
            "associatedSymptoms": { "type": "array", "items": { "type": "string" } },
            "severity": {
              "type": "integer",
              "minimum": 1,
              "maximum": 10
            },
            "location": { "type": "string" },
            "radiation": { "type": "boolean" },
            "radiationDetails":{ "type": "string" },
            "frequency": { "type": "string" },
            "onset": { "type": "string" },
            "context": { "type": "string" },
            "functionalImpact": { "type": "string" }
          }
        }
        """;

    private const string diagnosis = """
        "diagnosis": {
          "type": "object",
          "required": ["description"],
          "properties": {
            "description": { "type": "string" },
            "icd10": { "type": "string" },
            "type": {
              "type": "string",
              "enum": ["definitive", "presumptive", "differential"]
            }
          }
        }
        """;

    private const string treatmentPlan = """
        "treatmentPlan": {
          "type": "object",
          "properties": {
            "instructions": { "type": "string" },
            "medications": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" },
                  "dose": { "type": "string" },
                  "frequency": { "type": "string" },
                  "duration": { "type": "string" }
                }
              }
            },
            "orderedTests": { "type": "array", "items": { "type": "string" } },
            "referral": { "type": "string" },
            "nextAppointment": { "type": "string", "format": "date" }
          }
        }
        """;

    private const string physicalExam = """
        "physicalExam": {
          "type": "object",
          "properties": {
            "general": { "type": "string" },
            "findings": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "system": { "type": "string" },
                  "description": { "type": "string" }
                }
              }
            }
          }
        }
        """;

    private const string medicalBackground = """
        "medicalBackground": {
          "type": "object",
          "properties": {
            "pathological": { "type": "array", "items": { "type": "string" } },
            "surgical": { "type": "array", "items": { "type": "string" } },
            "allergies": { "type": "array", "items": { "type": "string" } },
            "familyHistory": { "type": "array", "items": { "type": "string" } },
            "currentMedications": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" },
                  "dose": { "type": "string" },
                  "frequency": { "type": "string" }
                }
              }
            }
          }
        }
        """;

    /// <summary>
    /// Gets the list of clinical form templates.
    /// </summary>
    /// <returns>A read-only list of <see cref="ClinicalFormTemplate"/>.</returns>
    public static IReadOnlyList<ClinicalFormTemplate> GetTemplates() =>
        [
            // General Medicine
            ClinicalFormTemplate.Create(
                "GEN_INTAKE_V1",
                "General Medicine Intake Form",
                "Complete intake form for general medicine consultations.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "vitalSigns", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    {{vitalSigns}},
                    {{anamnesis}},
                    {{medicalBackground}},
                    {{physicalExam}},
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Pediatrics
            ClinicalFormTemplate.Create(
                "PED_INTAKE_V1",
                "Pediatric Intake Form",
                "Intake form adapted for pediatric consultations including developmental data.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "vitalSigns", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    {{vitalSigns}},
                    {{anamnesis}},
                    "developmentalHistory": {
                      "type": "object",
                      "properties": {
                        "vaccines": { "type": "array", "items": { "type": "string" } },
                        "milestones": { "type": "string" },
                        "schoolPerformance":{ "type": "string" }
                      }
                    },
                    "familyBackground": {
                      "type": "object",
                      "properties": {
                        "hereditaryDiseases": { "type": "array", "items": { "type": "string" } },
                        "caregiver": { "type": "string" }
                      }
                    },
                    {{physicalExam}},
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Cardiology
            ClinicalFormTemplate.Create(
                "CARD_VITALS_V1",
                "Cardiology Vitals Form",
                "Records core cardiovascular vital signs and risk factors.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["heartRate", "bloodPressure", "diagnosis"],
                  "properties": {
                    "heartRate": { "type": "integer" },
                    "bloodPressure": { "type": "string" },
                    "oxygenSaturation": {
                      "type": "integer",
                      "minimum": 0,
                      "maximum": 100
                    },
                    "cardiacRhythm": { "type": "string" },
                    "heartSounds": { "type": "string" },
                    "peripheralEdema": { "type": "boolean" },
                    "dyspnea": { "type": "boolean" },
                    "exerciseTolerance":{ "type": "string" },
                    "chestPainDetails": {
                      "type": "object",
                      "properties": {
                        "type": { "type": "string" },
                        "intensity": { "type": "integer", "minimum": 1, "maximum": 10 },
                        "radiation": { "type": "string" },
                        "duration": { "type": "string" },
                        "triggeredBy": { "type": "string" }
                      }
                    },
                    "riskFactors": {
                      "type": "object",
                      "properties": {
                        "smoking": { "type": "boolean" },
                        "diabetes": { "type": "boolean" },
                        "hypertension": { "type": "boolean" },
                        "obesity": { "type": "boolean" },
                        "familyHistory": { "type": "boolean" }
                      }
                    },
                    "cardiovascularHistory": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            ClinicalFormTemplate.Create(
                "CARD_VITALS_V2",
                "Cardiology Vitals Form (Extended)",
                "Extended cardiovascular vitals — adds respiratory rate and ejection fraction.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["heartRate", "bloodPressure", "oxygenSaturation", "respiratoryRate", "diagnosis"],
                  "properties": {
                    "heartRate": { "type": "integer" },
                    "bloodPressure": { "type": "string" },
                    "oxygenSaturation": {
                      "type": "integer",
                      "minimum": 0,
                      "maximum": 100
                    },
                    "respiratoryRate": { "type": "integer" },
                    "cardiacRhythm": { "type": "string" },
                    "heartSounds": { "type": "string" },
                    "peripheralEdema": { "type": "boolean" },
                    "dyspnea": { "type": "boolean" },
                    "exerciseTolerance": { "type": "string" },
                    "ejectionFraction": { "type": "number" },
                    "chestPainDetails": {
                      "type": "object",
                      "properties": {
                        "type": { "type": "string" },
                        "intensity": { "type": "integer", "minimum": 1, "maximum": 10 },
                        "radiation": { "type": "string" },
                        "duration": { "type": "string" },
                        "triggeredBy": { "type": "string" }
                      }
                    },
                    "riskFactors": {
                      "type": "object",
                      "properties": {
                        "smoking": { "type": "boolean" },
                        "diabetes": { "type": "boolean" },
                        "hypertension": { "type": "boolean" },
                        "obesity": { "type": "boolean" },
                        "familyHistory": { "type": "boolean" }
                      }
                    },
                    "cardiovascularHistory": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            ClinicalFormTemplate.Create(
                "CARD_ECG_V1",
                "ECG Results Form",
                "Records electrocardiogram findings for cardiac evaluation.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["rhythm", "findings", "diagnosis"],
                  "properties": {
                    "performedAt": { "type": "string", "format": "date-time" },
                    "rhythm": { "type": "string" },
                    "heartRate": { "type": "integer" },
                    "axis": { "type": "string" },
                    "findings": { "type": "string" },
                    "prInterval": { "type": "number" },
                    "qrsDuration": { "type": "number" },
                    "qtCorrected": { "type": "number" },
                    "stChanges": { "type": "string" },
                    "conductionAbnormalities": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    "interpretation": { "type": "string" },
                    "comparisonWithPrevious":{ "type": "string" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Dermatology
            ClinicalFormTemplate.Create(
                "DERM_LESION_V1",
                "Dermatology Lesion Form",
                "Documents skin lesion characteristics for dermatological evaluation.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "lesionDescription", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "lesionDescription": {
                      "type": "object",
                      "required": ["location", "morphology"],
                      "properties": {
                        "location": { "type": "string" },
                        "morphology": { "type": "string" },
                        "size": { "type": "string" },
                        "color": { "type": "string" },
                        "surface": { "type": "string" },
                        "borders": { "type": "string" },
                        "evolution": { "type": "string" },
                        "distribution": { "type": "string" },
                        "numberOfLesions": { "type": "integer" }
                      }
                    },
                    "itchSeverity": {
                      "type": "integer",
                      "minimum": 0,
                      "maximum": 10
                    },
                    "associatedSymptoms": { "type": "array", "items": { "type": "string" } },
                    "dermoscopyFindings": { "type": "string" },
                    "suspectedMalignancy": { "type": "boolean" },
                    "biopsyRequired": { "type": "boolean" },
                    "photoDocumentation": { "type": "boolean" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Gynaecology
            ClinicalFormTemplate.Create(
                "GYN_CONSULT_V1",
                "Gynaecology Consultation Form",
                "General gynaecological consultation including obstetric background.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "pregnancyStatus": {
                      "type": "string",
                      "enum": ["pregnant", "not_pregnant", "unknown"]
                    },
                    "sexualActivity": { "type": "boolean" },
                    "pelvicPain": { "type": "boolean" },
                    "vaginalDischarge":{ "type": "string" },
                    {{vitalSigns}},
                    "menstrualCycle": {
                      "type": "object",
                      "properties": {
                        "regular": { "type": "boolean" },
                        "frequency": { "type": "string" },
                        "duration": { "type": "string" },
                        "flow": { "type": "string" }
                      }
                    },
                    "obstetricsBackground": {
                      "type": "object",
                      "properties": {
                        "lastMenstrualPeriod": { "type": "string", "format": "date" },
                        "pregnancies": { "type": "integer" },
                        "deliveries": { "type": "integer" },
                        "abortions": { "type": "integer" },
                        "contraceptives": { "type": "string" },
                        "lastPapSmear": { "type": "string", "format": "date" }
                      }
                    },
                    {{anamnesis}},
                    "gynecologicalExam": {
                      "type": "object",
                      "properties": {
                        "cervix": {
                          "type": "object",
                          "properties": {
                            "appearance": { "type": "string" },
                            "consistency": { "type": "string" },
                            "os": { "type": "string" }
                          }
                        },
                        "uterus": {
                          "type": "object",
                          "properties": {
                            "size": { "type": "string" },
                            "position": { "type": "string" },
                            "mobility": { "type": "string" },
                            "tenderness": { "type": "boolean" }
                          }
                        },
                        "adnexa": {
                          "type": "object",
                          "properties": {
                            "left": { "type": "string" },
                            "right": { "type": "string" },
                            "masses": { "type": "boolean" }
                          }
                        },
                        "discharge": {
                          "type": "object",
                          "properties": {
                            "color": { "type": "string" },
                            "odor": { "type": "string" },
                            "consistency": { "type": "string" }
                          }
                        },
                        "tenderness": { "type": "boolean" }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Ophthalmology
            ClinicalFormTemplate.Create(
                "OPH_EXAM_V1",
                "Ophthalmology Exam Form",
                "Records visual acuity, intraocular pressure and ocular findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "visualAcuity", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "externalExam": { "type": "string" },
                    "visualAcuity": {
                      "type": "object",
                      "required": ["leftEye", "rightEye"],
                      "properties": {
                        "leftEye": { "type": "string" },
                        "rightEye": { "type": "string" },
                        "withCorrection": { "type": "boolean" }
                      }
                    },
                    "pupilReaction": { "type": "string" },
                    "ocularMotility": { "type": "string" },
                    "visualField": { "type": "string" },
                    "intraocularPressure": {
                      "type": "object",
                      "properties": {
                        "leftEye": { "type": "number" },
                        "rightEye": { "type": "number" }
                      }
                    },
                    "fundusExam": { "type": "string" },
                    "slitLampFindings": { "type": "string" },
                    "colorVision": { "type": "string" },
                    "diagnosticImages": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Orthopedics
            ClinicalFormTemplate.Create(
                "ORT_MUSCULO_V1",
                "Orthopedic Musculoskeletal Form",
                "Evaluates musculoskeletal function, injury mechanism and imaging findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "affectedArea", "affectedSide", "painAssessment", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "affectedArea": { "type": "string" },
                    "affectedSide": {
                      "type": "string",
                      "enum": ["left", "right", "bilateral", "midline"]
                    },
                    "injuryMechanism": { "type": "string" },
                    "functionalLimitation":{ "type": "string" },
                    "swelling":   { "type": "boolean" },
                    "deformity":  { "type": "boolean" },
                    "weightBearing": {
                      "type": "string",
                      "enum": ["normal", "limited", "unable"]
                    },
                    "painAssessment": {
                      "type": "object",
                      "required": ["level"],
                      "properties": {
                        "level": { "type": "integer", "minimum": 0, "maximum": 10 },
                        "type": { "type": "string" },
                        "radiation": { "type": "boolean" }
                      }
                    },
                    "mobilityAssessment": {
                      "type": "object",
                      "properties": {
                        "rangeOfMotion": { "type": "string" },
                        "muscleStrength": { "type": "integer", "minimum": 0, "maximum": 5 },
                        "stability": { "type": "string" }
                      }
                    },
                    "neurologicalSymptoms": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    "imagingFindings": {
                      "type": "object",
                      "properties": {
                        "type": {
                          "type": "string",
                          "enum": ["xray", "mri", "ct", "ultrasound", "none"]
                        },
                        "findings": { "type": "string" }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Otolaryngology
            ClinicalFormTemplate.Create(
                "OTO_EXAM_V1",
                "ENT Examination Form",
                "Documents ear, nose and throat examination findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "entFindings", "diagnosis"],
                  "properties": {
                    "chiefComplaint":  { "type": "string", "minLength": 1 },
                    "symptomDuration": { "type": "string" },
                    "fever": { "type": "boolean" },
                    "vertigo": { "type": "boolean" },
                    "balanceIssues": { "type": "boolean" },
                    "voiceChanges": { "type": "boolean" },
                    "entFindings": {
                      "type": "object",
                      "required": ["ear", "nose", "throat"],
                      "properties": {
                        "ear": {
                          "type": "object",
                          "properties": {
                            "left": { "type": "string" },
                            "right": { "type": "string" },
                            "hearingLoss": { "type": "boolean" },
                            "tinnitus": { "type": "boolean" }
                          }
                        },
                        "nose": {
                          "type": "object",
                          "properties": {
                            "obstruction": { "type": "boolean" },
                            "discharge": { "type": "string" },
                            "findings": { "type": "string" }
                          }
                        },
                        "throat": {
                          "type": "object",
                          "properties": {
                            "tonsils": { "type": "string" },
                            "pharynx": { "type": "string" },
                            "larynx": { "type": "string" }
                          }
                        }
                      }
                    },
                    "lymphNodes": { "type": "string" },
                    "audiometryResults": { "type": "string" },
                    "nasalEndoscopyFindings": { "type": "string" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Neurology
            ClinicalFormTemplate.Create(
                "NEUR_REFLEX_V1",
                "Neurology Neurological Exam Form",
                "Records motor function, reflexes and cognitive evaluation.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "neurologicalExam", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "seizureHistory": { "type": "boolean" },
                    "headacheCharacteristics": { "type": "string" },
                    "neurologicalExam": {
                      "type": "object",
                      "required": ["motorFunction", "reflexes"],
                      "properties": {
                        "motorFunction": { "type": "string" },
                        "reflexes": { "type": "string" },
                        "sensitivity": { "type": "string" },
                        "sensoryDistribution": { "type": "string" },
                        "coordination": { "type": "string" },
                        "cranialNerves": { "type": "string" },
                        "gait": { "type": "string" },
                        "speech": { "type": "string" },
                        "cognitiveScore": { "type": "integer" },
                        "glasgowScale": {
                          "type": "integer",
                          "minimum": 3,
                          "maximum": 15
                        },
                        "consciousLevel": {
                          "type": "string",
                          "enum": ["alert", "drowsy", "stuporous", "comatose"]
                        }
                      }
                    },
                    "imagingResults": { "type": "string" },
                    {{anamnesis}},
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            ClinicalFormTemplate.Create(
                "NEUR_IMAGING_V1",
                "Neurology Imaging Form",
                "Records neurological imaging study results.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["imagingStudy", "diagnosis"],
                  "properties": {
                    "imagingStudy": {
                      "type": "object",
                      "required": ["type", "findings"],
                      "properties": {
                        "performedAt": { "type": "string", "format": "date-time" },
                        "type": {
                          "type": "string",
                          "enum": ["mri", "ct", "pet", "angiography"]
                        },
                        "laterality": {
                          "type": "string",
                          "enum": ["left", "right", "bilateral", "midline"]
                        },
                        "imageQuality": {
                          "type": "string",
                          "enum": ["adequate", "limited", "poor"]
                        },
                        "contrastUsed": { "type": "boolean" },
                        "affectedRegions": { "type": "array", "items": { "type": "string" } },
                        "findings": { "type": "string" },
                        "impression": { "type": "string" },
                        "incidentalFindings": { "type": "string" },
                        "comparisonStudy": { "type": "string" },
                        "urgency": {
                          "type": "string",
                          "enum": ["routine", "urgent", "emergency"]
                        }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Psychiatry
            ClinicalFormTemplate.Create(
                "PSYC_SESSION_V1",
                "Psychiatry Session Form",
                "Records mood, mental state and session notes for psychiatric consultations.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["mentalStateExam", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "mentalStateExam": {
                      "type": "object",
                      "required": ["mood", "sessionNotes"],
                      "properties": {
                        "mood": {
                          "type": "string",
                          "enum": ["depressed", "euthymic", "anxious", "irritable", "expansive", "mixed"]
                        },
                        "affect": { "type": "string" },
                        "orientation": { "type": "string" },
                        "thoughtProcess":{ "type": "string" },
                        "perception": { "type": "string" },
                        "cognition": { "type": "string" },
                        "insight": { "type": "string" },
                        "sessionNotes": { "type": "string" }
                      }
                    },
                    "anxietyLevel": {
                      "type": "integer",
                      "minimum": 0,
                      "maximum": 10
                    },
                    "sleepPattern": { "type": "string" },
                    "substanceUse": { "type": "string" },
                    "therapyType": { "type": "string" },
                    "riskAssessment": { "type": "string" },
                    "currentMedications": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    "followUpDate": { "type": "string", "format": "date" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            ClinicalFormTemplate.Create(
                "PSYC_RISK_V1",
                "Psychiatry Risk Assessment Form",
                "Evaluates patient risk level, suicidality and identified triggers.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["riskAssessment", "diagnosis"],
                  "properties": {
                    "riskAssessment": {
                      "type": "object",
                      "required": ["riskLevel", "suicidalIdeation"],
                      "properties": {
                        "riskLevel": {
                          "type": "string",
                          "enum": ["low", "moderate", "high", "critical"]
                        },
                        "suicidalIdeation": { "type": "boolean" },
                        "homicidalIdeation":{ "type": "boolean" },
                        "triggers": { "type": "array", "items": { "type": "string" } },
                        "protectiveFactors":{ "type": "array", "items": { "type": "string" } },
                        "safetyPlan": { "type": "string" }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Urology
            ClinicalFormTemplate.Create(
                "URO_CONSULT_V1",
                "Urology Consultation Form",
                "General urological consultation including urinary and sexual health.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "urinarySymptoms", "diagnosis"],
                  "properties": {
                    "chiefComplaint":    { "type": "string", "minLength": 1 },
                    "biologicalSex": {
                      "type": "string",
                      "enum": ["male", "female", "intersex", "unknown"]
                    },
                    "urinarySymptoms": {
                      "type": "object",
                      "required": ["description"],
                      "properties": {
                        "description": { "type": "string" },
                        "urinaryFrequency": { "type": "string" },
                        "urgency": { "type": "boolean" },
                        "dysuria": { "type": "boolean" },
                        "hematuria": { "type": "boolean" },
                        "incontinence": { "type": "boolean" },
                        "nocturia": { "type": "boolean" },
                        "weakStream": { "type": "boolean" },
                        "retention": { "type": "boolean" }
                      }
                    },
                    "flankPain": {
                      "type": "object",
                      "properties": {
                        "present": { "type": "boolean" },
                        "laterality": {
                          "type": "string",
                          "enum": ["left", "right", "bilateral"]
                        },
                        "severity": {
                          "type": "integer",
                          "minimum": 0,
                          "maximum": 10
                        }
                      }
                    },
                    "catheterUse": { "type": "boolean" },
                    "urinalysis": {
                      "type": "object",
                      "properties": {
                        "leukocytes": { "type": "string" },
                        "nitrites": { "type": "boolean" },
                        "proteins": { "type": "string" },
                        "blood": { "type": "string" },
                        "bacteria": { "type": "string" },
                        "glucose": { "type": "string" },
                        "ph": { "type": "number" },
                        "appearance": { "type": "string" }
                      }
                    },
                    "urologicalExam": {
                      "type": "object",
                      "properties": {
                        "flankMasses": { "type": "boolean" },
                        "lumbosacralTenderness": { "type": "boolean" },
                        "maleExam": {
                          "type": "object",
                          "properties": {
                            "prostateExam": { "type": "string" },
                            "psa": { "type": "number" },
                            "testicularExam": {
                              "type": "object",
                              "properties": {
                                "left": { "type": "string" },
                                "right": { "type": "string" },
                                "tenderness": { "type": "boolean" },
                                "masses": { "type": "boolean" }
                              }
                            }
                          }
                        },
                        "femaleExam": {
                          "type": "object",
                          "properties": {
                            "pelvicExam": { "type": "string" },
                            "dyspareunia": { "type": "boolean" },
                            "pelvicMasses": { "type": "boolean" }
                          }
                        }
                      }
                    },
                    "sexualHealth": {
                      "type": "object",
                      "properties": {
                        "erectileDysfunction": { "type": "boolean" },
                        "decreasedLibido": { "type": "boolean" },
                        "infertilityConcerns": { "type": "boolean" }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Oncology
            ClinicalFormTemplate.Create(
                "ONCO_FOLLOWUP_V1",
                "Oncology Follow-up Form",
                "Tracks tumor markers, treatment response and toxicity over time.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["oncologySummary", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "cancerStage": {
                      "type": "object",
                      "properties": {
                        "tnmT": { "type": "string" },
                        "tnmN": { "type": "string" },
                        "tnmM": { "type": "string" },
                        "stage": {
                          "type": "string",
                          "enum": ["I", "IA", "IB", "II", "IIA", "IIB", "III", "IIIA", "IIIB", "IIIC", "IV"]
                        },
                        "subtype": { "type": "string" }
                      }
                    },
                    "metastasis":   { "type": "boolean" },
                    "affectedSites":{ "type": "array", "items": { "type": "string" } },
                    "performanceStatus": {
                      "type": "integer",
                      "minimum": 0,
                      "maximum": 4
                    },
                    "weightChange":     { "type": "string" },
                    "hospitalizations": { "type": "boolean" },
                    "timeline": {
                      "type": "object",
                      "properties": {
                        "diagnosisDate": { "type": "string", "format": "date" },
                        "treatmentStartDate": { "type": "string", "format": "date" },
                        "lastEvaluationDate": { "type": "string", "format": "date" },
                        "nextEvaluationDate": { "type": "string", "format": "date" }
                      }
                    },
                    "molecularProfile": {
                      "type": "object",
                      "properties": {
                        "her2": { "type": "string" },
                        "egfr": { "type": "string" },
                        "pdl1": { "type": "string" },
                        "kras": { "type": "string" },
                        "other": { "type": "array", "items": { "type": "string" } }
                      }
                    },
                    "oncologySummary": {
                      "type": "object",
                      "required": ["treatmentType", "treatmentIntent", "treatmentResponse"],
                      "properties": {
                        "treatmentType": {
                          "type": "string",
                          "enum": [
                            "chemotherapy",
                            "radiotherapy",
                            "immunotherapy",
                            "targeted",
                            "surgery",
                            "hormonal"
                          ]
                        },
                        "treatmentIntent": {
                          "type": "string",
                          "enum": ["curative", "palliative", "adjuvant", "neoadjuvant"]
                        },
                        "treatmentResponse": {
                          "type": "string",
                          "enum": ["complete", "partial", "stable", "progressive"]
                        },
                        "currentCycle": { "type": "integer" },
                        "nextCycleDate": { "type": "string", "format": "date" },
                        "toxicityGrade": {
                          "type": "integer",
                          "minimum": 0,
                          "maximum": 4
                        },
                        "sideEffects": {
                          "type": "array",
                          "items": { "type": "string" }
                        }
                      }
                    },
                    "tumorMarkers": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "required": ["name", "value", "unit"],
                        "properties": {
                          "name": { "type": "string" },
                          "value": { "type": "number" },
                          "unit": { "type": "string" },
                          "trend": {
                            "type": "string",
                            "enum": ["rising", "stable", "falling"]
                          }
                        }
                      }
                    },
                    "imagingFollowUp": {
                      "type": "object",
                      "properties": {
                        "modality": { "type": "string" },
                        "tumorSize": { "type": "string" },
                        "newLesions": { "type": "boolean" },
                        "metastasisSites": { "type": "array", "items": { "type": "string" } },
                        "progression": { "type": "boolean" },
                        "findings": { "type": "string" }
                      }
                    },
                    "labResults": { "type": "string" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Endocrinology
            ClinicalFormTemplate.Create(
                "ENDO_HORMONAL_V1",
                "Endocrinology Hormonal Form",
                "Records hormonal levels, metabolic indicators and endocrine findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "biologicalSex", "metabolicPanel", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "biologicalSex": {
                      "type": "string",
                      "enum": ["male", "female", "intersex", "unknown"]
                    },
                    "weightHistory": { "type": "string" },
                    "associatedSymptoms": { "type": "array", "items": { "type": "string" } },
                    {{vitalSigns}},
                    "bmi": { "type": "number" },
                    "thyroidExam": { "type": "string" },
                    "metabolicPanel": {
                      "type": "object",
                      "required": ["glucose"],
                      "properties": {
                        "glucose": {
                          "type": "object",
                          "required": ["value"],
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "hba1c": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "insulin": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        }
                      }
                    },
                    "thyroidPanel": {
                      "type": "object",
                      "properties": {
                        "tsh": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "t3": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "t4": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        }
                      }
                    },
                    "adrenalPanel": {
                      "type": "object",
                      "properties": {
                        "cortisol": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        }
                      }
                    },
                    "sexHormones": {
                      "type": "object",
                      "properties": {
                        "testosterone": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "estradiol": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Gastroenterology
            ClinicalFormTemplate.Create(
                "GAST_DIGEST_V1",
                "Gastroenterology Digestive Form",
                "Documents digestive symptoms, endoscopic and imaging findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "digestiveSymptoms", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "digestiveSymptoms": {
                      "type": "object",
                      "required": ["description"],
                      "properties": {
                        "description": { "type": "string" },
                        "nausea": { "type": "boolean" },
                        "vomiting": { "type": "boolean" },
                        "diarrhea": { "type": "boolean" },
                        "constipation": { "type": "boolean" },
                        "rectalBleeding": { "type": "boolean" },
                        "bloating": { "type": "boolean" },
                        "reflux": { "type": "boolean" },
                        "weightLoss": { "type": "boolean" },
                        "appetiteChanges":  { "type": "string" },
                        "foodRelation":     { "type": "string" },
                        "stoolCharacteristics": { "type": "string" }
                      }
                    },
                    "abdominalPain": {
                      "type": "object",
                      "properties": {
                        "present": { "type": "boolean" },
                        "location": {
                          "type": "string",
                          "enum": [
                            "epigastric",
                            "periumbilical",
                            "rightUpperQuadrant",
                            "leftUpperQuadrant",
                            "rightLowerQuadrant",
                            "leftLowerQuadrant",
                            "diffuse"
                          ]
                        },
                        "type": { "type": "string" },
                        "severity": {
                          "type": "integer",
                          "minimum": 0,
                          "maximum": 10
                        },
                        "radiation": { "type": "string" },
                        "timing": { "type": "string" }
                      }
                    },
                    "abdomenExam": {
                      "type": "object",
                      "properties": {
                        "tenderness": { "type": "string" },
                        "organomegaly": { "type": "string" },
                        "bowelSounds": { "type": "string" },
                        "ascites": { "type": "boolean" },
                        "rigidity": { "type": "boolean" }
                      }
                    },
                    "liverFunctionTests": {
                      "type": "object",
                      "properties": {
                        "ast": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "alt": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "bilirubin": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "alp": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        },
                        "albumin": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "referenceRange": { "type": "string" }
                          }
                        }
                      }
                    },
                    "endoscopyFindings": { "type": "string" },
                    "imagingFindings": { "type": "string" },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
            // Pulmonology
            ClinicalFormTemplate.Create(
                "PULM_RESPIRATORY_V1",
                "Pulmonology Respiratory Form",
                "Records respiratory function, auscultation and pulmonary findings.",
                $$"""
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "required": ["chiefComplaint", "respiratoryAssessment", "diagnosis"],
                  "properties": {
                    "chiefComplaint": { "type": "string", "minLength": 1 },
                    "smokingHistory": {
                      "type": "object",
                      "properties": {
                        "status": {
                          "type": "string",
                          "enum": ["current", "former", "never"]
                        },
                        "packYears": { "type": "number" }
                      }
                    },
                    "respiratoryAssessment": {
                      "type": "object",
                      "required": ["oxygenSaturation", "breathingSounds"],
                      "properties": {
                        "oxygenSaturation": {
                          "type": "integer",
                          "minimum": 0,
                          "maximum": 100
                        },
                        "respiratoryRate": { "type": "integer" },
                        "breathingSounds": { "type": "string" },
                        "dyspnea": { "type": "boolean" },
                        "cough": { "type": "boolean" },
                        "hemoptysis": { "type": "boolean" },
                        "wheezing": { "type": "boolean" },
                        "sputumProduction": { "type": "string" },
                        "exerciseTolerance":{ "type": "string" },
                        "oxygenTherapy": { "type": "boolean" },
                        "chestPain": {
                          "type": "object",
                          "properties": {
                            "present": { "type": "boolean" },
                            "type": { "type": "string" },
                            "pleuritic": { "type": "boolean" }
                          }
                        }
                      }
                    },
                    "sleepSymptoms": {
                      "type": "object",
                      "properties": {
                        "snoring": { "type": "boolean" },
                        "apneaEvents": { "type": "boolean" },
                        "daytimeSleepiness": {
                          "type": "integer",
                          "minimum": 0,
                          "maximum": 10
                        }
                      }
                    },
                    "spirometryResults": {
                      "type": "object",
                      "properties": {
                        "fev1": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "percentPredicted": { "type": "number" }
                          }
                        },
                        "fvc": {
                          "type": "object",
                          "properties": {
                            "value": { "type": "number" },
                            "unit": { "type": "string" },
                            "percentPredicted": { "type": "number" }
                          }
                        },
                        "fev1FvcRatio": { "type": "number" },
                        "interpretation": {
                          "type": "string",
                          "enum": ["normal", "obstructive", "restrictive", "mixed"]
                        }
                      }
                    },
                    "imagingFindings": {
                      "type": "object",
                      "properties": {
                        "modality": { "type": "string" },
                        "infiltrates": { "type": "boolean" },
                        "nodules": { "type": "boolean" },
                        "fibrosis": { "type": "boolean" },
                        "pleuralEffusion": { "type": "boolean" },
                        "findings": { "type": "string" }
                      }
                    },
                    {{diagnosis}},
                    {{treatmentPlan}},
                    "additionalNotes": { "type": "string" }
                  }
                }
                """
            ),
        ];
};
