namespace ClinicFlow.Infrastructure.Persistence.Seeding.Data;

/// <summary>
/// Provides realistic sample JSON payloads per clinical form template code for seeding.
/// Each payload conforms to its corresponding template's JSON schema.
/// </summary>
public static class ClinicalDetailSampleData
{
    private static readonly Dictionary<string, string[]> SamplesByTemplateCode = new()
    {
        ["GEN_INTAKE_V1"] =
        [
            """
                {
                  "chiefComplaint": "Persistent fatigue and low-grade fever for 2 weeks",
                  "vitalSigns": {
                    "bloodPressure": "130/85",
                    "heartRate": 78,
                    "temperature": 37.4,
                    "weight": 72.5,
                    "height": 170
                  },
                  "anamnesis": {
                    "evolutionTime": "2 weeks",
                    "characteristics": "Gradual onset fatigue",
                    "associatedSymptoms": [
                      "headache",
                      "myalgia"
                    ],
                    "severity": 4
                  },
                  "diagnosis": {
                    "description": "Viral syndrome",
                    "icd10": "B34.9",
                    "type": "presumptive"
                  },
                  "treatmentPlan": {
                    "instructions": "Rest, hydration, symptomatic relief",
                    "medications": [
                      {
                        "name": "Acetaminophen",
                        "dose": "500mg",
                        "frequency": "Every 8 hours",
                        "duration": "5 days"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Annual checkup — no active complaints",
                  "vitalSigns": {
                    "bloodPressure": "120/80",
                    "heartRate": 72,
                    "temperature": 36.6,
                    "weight": 68,
                    "height": 165
                  },
                  "diagnosis": {
                    "description": "Routine health examination — no abnormalities detected",
                    "icd10": "Z00.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Continue current lifestyle. Return in 12 months.",
                    "orderedTests": [
                      "CBC",
                      "Lipid panel",
                      "Fasting glucose"
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Recurrent sore throat with mild dysphagia",
                  "vitalSigns": {
                    "bloodPressure": "118/76",
                    "heartRate": 80,
                    "temperature": 37.8,
                    "weight": 75,
                    "height": 172
                  },
                  "anamnesis": {
                    "evolutionTime": "5 days",
                    "characteristics": "Sharp throat pain on swallowing",
                    "associatedSymptoms": [
                      "cough",
                      "rhinorrhea"
                    ],
                    "severity": 5
                  },
                  "physicalExam": {
                    "general": "Mildly ill-appearing",
                    "findings": [
                      {
                        "system": "HEENT",
                        "description": "Erythematous pharynx, no tonsillar exudate"
                      }
                    ]
                  },
                  "diagnosis": {
                    "description": "Acute pharyngitis",
                    "icd10": "J02.9",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Warm fluids, gargle with saline",
                    "medications": [
                      {
                        "name": "Ibuprofen",
                        "dose": "400mg",
                        "frequency": "Every 8 hours",
                        "duration": "3 days"
                      }
                    ]
                  }
                }
                """,
        ],
        ["PED_INTAKE_V1"] =
        [
            """
                {
                  "chiefComplaint": "Routine well-child visit — 4-year-old",
                  "vitalSigns": {
                    "bloodPressure": "90/60",
                    "heartRate": 100,
                    "temperature": 36.5,
                    "weight": 16.5,
                    "height": 102
                  },
                  "developmentalHistory": {
                    "vaccines": [
                      "DTaP",
                      "IPV",
                      "MMR",
                      "Varicella"
                    ],
                    "milestones": "Age-appropriate speech and motor skills",
                    "schoolPerformance": "N/A — pre-school"
                  },
                  "diagnosis": {
                    "description": "Well-child visit — normal development",
                    "icd10": "Z00.12",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Continue vaccination schedule. Follow-up in 12 months."
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Recurring ear pain — 6-year-old",
                  "vitalSigns": {
                    "bloodPressure": "95/60",
                    "heartRate": 95,
                    "temperature": 38.1,
                    "weight": 22,
                    "height": 118
                  },
                  "anamnesis": {
                    "evolutionTime": "3 days",
                    "characteristics": "Bilateral ear pain, worse at night",
                    "associatedSymptoms": [
                      "fever",
                      "irritability"
                    ],
                    "severity": 6
                  },
                  "familyBackground": {
                    "hereditaryDiseases": [
                      "Asthma"
                    ],
                    "caregiver": "Mother"
                  },
                  "diagnosis": {
                    "description": "Acute bilateral otitis media",
                    "icd10": "H66.93",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Complete antibiotic course, follow-up in 10 days",
                    "medications": [
                      {
                        "name": "Amoxicillin",
                        "dose": "250mg/5ml",
                        "frequency": "Every 8 hours",
                        "duration": "10 days"
                      }
                    ]
                  }
                }
                """,
        ],
        ["CARD_VITALS_V1"] =
        [
            """
                {
                  "heartRate": 82,
                  "bloodPressure": "145/92",
                  "oxygenSaturation": 97,
                  "cardiacRhythm": "Regular sinus rhythm",
                  "heartSounds": "S1 S2 normal, no murmurs",
                  "peripheralEdema": false,
                  "dyspnea": false,
                  "riskFactors": {
                    "smoking": false,
                    "diabetes": true,
                    "hypertension": true,
                    "obesity": false,
                    "familyHistory": true
                  },
                  "diagnosis": {
                    "description": "Essential hypertension, stage 2",
                    "icd10": "I10",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Low-sodium diet, regular exercise",
                    "medications": [
                      {
                        "name": "Losartan",
                        "dose": "50mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "heartRate": 54,
                  "bloodPressure": "128/84",
                  "oxygenSaturation": 98,
                  "cardiacRhythm": "Sinus bradycardia",
                  "heartSounds": "S1 S2 normal",
                  "peripheralEdema": false,
                  "dyspnea": true,
                  "exerciseTolerance": "Moderate — NYHA class II",
                  "chestPainDetails": {
                    "type": "Anginal",
                    "intensity": 3,
                    "duration": "5 minutes",
                    "triggeredBy": "Exertion"
                  },
                  "riskFactors": {
                    "smoking": true,
                    "diabetes": false,
                    "hypertension": true,
                    "obesity": false,
                    "familyHistory": false
                  },
                  "diagnosis": {
                    "description": "Chronic stable angina",
                    "icd10": "I20.8",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Smoking cessation counseling, stress test ordered",
                    "medications": [
                      {
                        "name": "Aspirin",
                        "dose": "100mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      },
                      {
                        "name": "Atenolol",
                        "dose": "50mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
        ],
        ["CARD_VITALS_V2"] =
        [
            """
                {
                  "heartRate": 76,
                  "bloodPressure": "138/88",
                  "oxygenSaturation": 96,
                  "respiratoryRate": 18,
                  "cardiacRhythm": "Normal sinus rhythm",
                  "heartSounds": "S1 S2 normal, systolic murmur grade II/VI",
                  "peripheralEdema": true,
                  "dyspnea": true,
                  "exerciseTolerance": "Limited — NYHA class III",
                  "ejectionFraction": 35,
                  "riskFactors": {
                    "smoking": false,
                    "diabetes": true,
                    "hypertension": true,
                    "obesity": true,
                    "familyHistory": true
                  },
                  "cardiovascularHistory": [
                    "Previous MI 2023",
                    "Stent placement LAD"
                  ],
                  "diagnosis": {
                    "description": "Heart failure with reduced ejection fraction",
                    "icd10": "I50.2",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Fluid restriction 1.5L/day, daily weight monitoring",
                    "medications": [
                      {
                        "name": "Enalapril",
                        "dose": "10mg",
                        "frequency": "Twice daily",
                        "duration": "Ongoing"
                      },
                      {
                        "name": "Furosemide",
                        "dose": "40mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
        ],
        ["CARD_ECG_V1"] =
        [
            """
                {
                  "rhythm": "Normal sinus rhythm",
                  "heartRate": 74,
                  "axis": "Normal",
                  "findings": "No ST-segment changes, normal P waves",
                  "prInterval": 0.16,
                  "qrsDuration": 0.08,
                  "qtCorrected": 0.42,
                  "stChanges": "None",
                  "interpretation": "Normal ECG",
                  "diagnosis": {
                    "description": "Normal electrocardiogram",
                    "icd10": "Z01.81",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "No intervention required. Follow-up as needed."
                  }
                }
                """,
            """
                {
                  "rhythm": "Atrial fibrillation",
                  "heartRate": 112,
                  "axis": "Left axis deviation",
                  "findings": "Irregularly irregular rhythm, no ST elevation",
                  "qrsDuration": 0.09,
                  "qtCorrected": 0.44,
                  "stChanges": "Non-specific ST depression in V5-V6",
                  "conductionAbnormalities": [
                    "Left anterior fascicular block"
                  ],
                  "interpretation": "New-onset atrial fibrillation with rapid ventricular response",
                  "diagnosis": {
                    "description": "Atrial fibrillation — new onset",
                    "icd10": "I48.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Rate control and anticoagulation evaluation",
                    "medications": [
                      {
                        "name": "Metoprolol",
                        "dose": "50mg",
                        "frequency": "Twice daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
        ],
        ["DERM_LESION_V1"] =
        [
            """
                {
                  "chiefComplaint": "New pigmented mole on upper back",
                  "lesionDescription": {
                    "location": "Upper back, left paravertebral",
                    "morphology": "Asymmetric macule",
                    "size": "8mm",
                    "color": "Dark brown with irregular borders",
                    "surface": "Smooth",
                    "borders": "Irregular",
                    "evolution": "Growing over 3 months",
                    "numberOfLesions": 1
                  },
                  "suspectedMalignancy": true,
                  "biopsyRequired": true,
                  "photoDocumentation": true,
                  "dermoscopyFindings": "Atypical pigment network with blue-white veil",
                  "diagnosis": {
                    "description": "Suspicious melanocytic lesion — biopsy pending",
                    "icd10": "D48.5",
                    "type": "presumptive"
                  },
                  "treatmentPlan": {
                    "instructions": "Excisional biopsy scheduled. Avoid sun exposure."
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Itchy red patches on forearms",
                  "lesionDescription": {
                    "location": "Bilateral forearms",
                    "morphology": "Erythematous plaques",
                    "size": "3-5cm",
                    "color": "Red",
                    "surface": "Scaly",
                    "borders": "Well-defined",
                    "distribution": "Symmetric",
                    "numberOfLesions": 4
                  },
                  "itchSeverity": 7,
                  "associatedSymptoms": [
                    "pruritus",
                    "dryness"
                  ],
                  "suspectedMalignancy": false,
                  "biopsyRequired": false,
                  "photoDocumentation": false,
                  "diagnosis": {
                    "description": "Chronic plaque psoriasis",
                    "icd10": "L40.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Apply topical corticosteroid twice daily",
                    "medications": [
                      {
                        "name": "Betamethasone cream 0.05%",
                        "dose": "Thin layer",
                        "frequency": "Twice daily",
                        "duration": "4 weeks"
                      }
                    ]
                  }
                }
                """,
        ],
        ["GYN_CONSULT_V1"] =
        [
            """
                {
                  "chiefComplaint": "Irregular menstrual cycles and pelvic discomfort",
                  "pregnancyStatus": "not_pregnant",
                  "sexualActivity": true,
                  "pelvicPain": true,
                  "vitalSigns": {
                    "bloodPressure": "110/70",
                    "heartRate": 74,
                    "temperature": 36.5,
                    "weight": 62,
                    "height": 160
                  },
                  "menstrualCycle": {
                    "regular": false,
                    "frequency": "35-50 days",
                    "duration": "7 days",
                    "flow": "Heavy"
                  },
                  "obstetricsBackground": {
                    "pregnancies": 0,
                    "deliveries": 0,
                    "abortions": 0,
                    "contraceptives": "None",
                    "lastPapSmear": "2025-03-15"
                  },
                  "diagnosis": {
                    "description": "Polycystic ovary syndrome — suspected",
                    "icd10": "E28.2",
                    "type": "differential"
                  },
                  "treatmentPlan": {
                    "instructions": "Pelvic ultrasound and hormonal panel ordered",
                    "orderedTests": [
                      "Pelvic US",
                      "FSH",
                      "LH",
                      "Testosterone",
                      "DHEA-S"
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Routine prenatal checkup — 24 weeks",
                  "pregnancyStatus": "pregnant",
                  "sexualActivity": true,
                  "pelvicPain": false,
                  "vitalSigns": {
                    "bloodPressure": "115/72",
                    "heartRate": 82,
                    "temperature": 36.4,
                    "weight": 70,
                    "height": 163
                  },
                  "menstrualCycle": {
                    "regular": true,
                    "frequency": "28 days",
                    "duration": "5 days",
                    "flow": "Normal"
                  },
                  "obstetricsBackground": {
                    "lastMenstrualPeriod": "2025-12-10",
                    "pregnancies": 2,
                    "deliveries": 1,
                    "abortions": 0,
                    "contraceptives": "None"
                  },
                  "diagnosis": {
                    "description": "Normal pregnancy — second trimester",
                    "icd10": "Z34.82",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Continue prenatal vitamins. Glucose tolerance test at 28 weeks.",
                    "orderedTests": [
                      "Glucose tolerance test",
                      "CBC"
                    ],
                    "nextAppointment": "2026-07-15"
                  }
                }
                """,
        ],
        ["OPH_EXAM_V1"] =
        [
            """
                {
                  "chiefComplaint": "Progressive blurry vision — both eyes",
                  "visualAcuity": {
                    "leftEye": "20/40",
                    "rightEye": "20/50",
                    "withCorrection": true
                  },
                  "pupilReaction": "Equal, round, reactive to light",
                  "ocularMotility": "Full range — no restriction",
                  "intraocularPressure": {
                    "leftEye": 16,
                    "rightEye": 17
                  },
                  "fundusExam": "Normal optic discs, no papilledema",
                  "slitLampFindings": "Early nuclear sclerosis bilaterally",
                  "diagnosis": {
                    "description": "Age-related nuclear cataract — bilateral",
                    "icd10": "H25.1",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Updated corrective lens prescription. Cataract surgery evaluation in 6 months."
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Eye redness and discharge — left eye",
                  "visualAcuity": {
                    "leftEye": "20/25",
                    "rightEye": "20/20",
                    "withCorrection": false
                  },
                  "pupilReaction": "Normal",
                  "externalExam": "Left conjunctival injection, mucopurulent discharge",
                  "slitLampFindings": "Papillary reaction on tarsal conjunctiva",
                  "diagnosis": {
                    "description": "Bacterial conjunctivitis — left eye",
                    "icd10": "H10.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Apply antibiotic drops for 7 days. Avoid contact lens use.",
                    "medications": [
                      {
                        "name": "Tobramycin 0.3% drops",
                        "dose": "1 drop",
                        "frequency": "Every 4 hours",
                        "duration": "7 days"
                      }
                    ]
                  }
                }
                """,
        ],
        ["ORT_MUSCULO_V1"] =
        [
            """
                {
                  "chiefComplaint": "Right knee pain after sports injury",
                  "affectedArea": "Knee",
                  "affectedSide": "right",
                  "injuryMechanism": "Twisting injury during football",
                  "painAssessment": {
                    "level": 7,
                    "type": "Sharp",
                    "radiation": false
                  },
                  "swelling": true,
                  "deformity": false,
                  "weightBearing": "limited",
                  "mobilityAssessment": {
                    "rangeOfMotion": "Flexion limited to 90 degrees",
                    "muscleStrength": 4,
                    "stability": "Anterior drawer test positive"
                  },
                  "imagingFindings": {
                    "type": "mri",
                    "findings": "Partial ACL tear, no meniscal damage"
                  },
                  "diagnosis": {
                    "description": "Partial anterior cruciate ligament tear — right knee",
                    "icd10": "S83.511",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Brace and rehabilitation for 6 weeks. Re-evaluate for surgical candidacy.",
                    "medications": [
                      {
                        "name": "Naproxen",
                        "dose": "500mg",
                        "frequency": "Twice daily",
                        "duration": "10 days"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Low back pain radiating to left leg",
                  "affectedArea": "Lumbar spine",
                  "affectedSide": "midline",
                  "painAssessment": {
                    "level": 6,
                    "type": "Burning",
                    "radiation": true
                  },
                  "swelling": false,
                  "deformity": false,
                  "weightBearing": "normal",
                  "functionalLimitation": "Cannot sit for more than 30 minutes",
                  "mobilityAssessment": {
                    "rangeOfMotion": "Flexion limited",
                    "muscleStrength": 5,
                    "stability": "Normal"
                  },
                  "neurologicalSymptoms": [
                    "Tingling in left foot",
                    "Decreased ankle reflex"
                  ],
                  "imagingFindings": {
                    "type": "mri",
                    "findings": "L4-L5 disc herniation with left foraminal stenosis"
                  },
                  "diagnosis": {
                    "description": "Lumbar disc herniation L4-L5 with left radiculopathy",
                    "icd10": "M51.16",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Physical therapy 3x/week for 6 weeks. Epidural injection if no improvement.",
                    "medications": [
                      {
                        "name": "Pregabalin",
                        "dose": "75mg",
                        "frequency": "Twice daily",
                        "duration": "4 weeks"
                      }
                    ]
                  }
                }
                """,
        ],
        ["OTO_EXAM_V1"] =
        [
            """
                {
                  "chiefComplaint": "Progressive hearing loss — left ear",
                  "symptomDuration": "6 months",
                  "fever": false,
                  "vertigo": false,
                  "voiceChanges": false,
                  "entFindings": {
                    "ear": {
                      "left": "Retracted tympanic membrane, amber effusion",
                      "right": "Normal",
                      "hearingLoss": true,
                      "tinnitus": true
                    },
                    "nose": {
                      "obstruction": false,
                      "findings": "Normal mucosa"
                    },
                    "throat": {
                      "tonsils": "Normal",
                      "pharynx": "Clear",
                      "larynx": "Normal"
                    }
                  },
                  "audiometryResults": "Left-sided conductive hearing loss, 35dB at speech frequencies",
                  "diagnosis": {
                    "description": "Serous otitis media — left ear",
                    "icd10": "H65.22",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Nasal steroid spray, decongestant trial. Re-evaluate in 6 weeks.",
                    "medications": [
                      {
                        "name": "Fluticasone nasal spray",
                        "dose": "2 sprays per nostril",
                        "frequency": "Once daily",
                        "duration": "6 weeks"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Chronic nasal congestion and facial pressure",
                  "symptomDuration": "3 months",
                  "fever": false,
                  "vertigo": false,
                  "voiceChanges": false,
                  "entFindings": {
                    "ear": {
                      "left": "Normal",
                      "right": "Normal",
                      "hearingLoss": false,
                      "tinnitus": false
                    },
                    "nose": {
                      "obstruction": true,
                      "discharge": "Mucopurulent",
                      "findings": "Bilateral inferior turbinate hypertrophy"
                    },
                    "throat": {
                      "tonsils": "Grade I",
                      "pharynx": "Postnasal drip",
                      "larynx": "Normal"
                    }
                  },
                  "nasalEndoscopyFindings": "Bilateral mucosal edema, middle meatus purulence",
                  "diagnosis": {
                    "description": "Chronic rhinosinusitis without nasal polyps",
                    "icd10": "J32.9",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Saline irrigations, topical steroid, antibiotic course",
                    "medications": [
                      {
                        "name": "Amoxicillin-Clavulanate",
                        "dose": "875mg",
                        "frequency": "Twice daily",
                        "duration": "14 days"
                      },
                      {
                        "name": "Mometasone nasal spray",
                        "dose": "2 sprays per nostril",
                        "frequency": "Once daily",
                        "duration": "3 months"
                      }
                    ]
                  }
                }
                """,
        ],
        ["NEUR_REFLEX_V1"] =
        [
            """
                {
                  "chiefComplaint": "Recurrent severe headaches with visual aura",
                  "seizureHistory": false,
                  "headacheCharacteristics": "Unilateral throbbing, preceded by zigzag visual aura lasting 20 min",
                  "neurologicalExam": {
                    "motorFunction": "5/5 all extremities",
                    "reflexes": "2+ symmetric",
                    "sensitivity": "Intact",
                    "coordination": "Normal finger-to-nose",
                    "cranialNerves": "Intact CN II-XII",
                    "gait": "Normal",
                    "consciousLevel": "alert"
                  },
                  "anamnesis": {
                    "evolutionTime": "2 years",
                    "characteristics": "Pulsating, unilateral",
                    "severity": 8,
                    "frequency": "3-4 episodes per month",
                    "associatedSymptoms": [
                      "photophobia",
                      "nausea",
                      "phonophobia"
                    ]
                  },
                  "diagnosis": {
                    "description": "Migraine with aura",
                    "icd10": "G43.1",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Migraine diary, trigger avoidance, prophylaxis initiated",
                    "medications": [
                      {
                        "name": "Topiramate",
                        "dose": "25mg",
                        "frequency": "Once daily",
                        "duration": "3 months"
                      },
                      {
                        "name": "Sumatriptan",
                        "dose": "50mg",
                        "frequency": "As needed for acute attacks",
                        "duration": "As needed"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Tremor in right hand, progressive over 8 months",
                  "seizureHistory": false,
                  "neurologicalExam": {
                    "motorFunction": "Resting tremor right hand, cogwheel rigidity",
                    "reflexes": "2+ symmetric",
                    "sensitivity": "Intact",
                    "coordination": "Mild bradykinesia",
                    "cranialNerves": "Intact",
                    "gait": "Reduced arm swing on right",
                    "speech": "Hypophonic",
                    "consciousLevel": "alert"
                  },
                  "diagnosis": {
                    "description": "Parkinsonism — early stage, pending confirmatory workup",
                    "icd10": "G20",
                    "type": "presumptive"
                  },
                  "treatmentPlan": {
                    "instructions": "DaTscan ordered. Occupational therapy referral.",
                    "orderedTests": [
                      "DaTscan",
                      "Brain MRI"
                    ]
                  }
                }
                """,
        ],
        ["NEUR_IMAGING_V1"] =
        [
            """
                {
                  "imagingStudy": {
                    "type": "mri",
                    "performedAt": "2026-05-20T10:00:00Z",
                    "findings": "No acute intracranial abnormality. Mild periventricular white matter changes consistent with small vessel disease.",
                    "laterality": "bilateral",
                    "imageQuality": "adequate",
                    "contrastUsed": true,
                    "affectedRegions": [
                      "periventricular white matter"
                    ],
                    "impression": "Age-appropriate small vessel ischemic changes. No mass or hemorrhage.",
                    "urgency": "routine"
                  },
                  "diagnosis": {
                    "description": "Cerebral small vessel disease — age-related",
                    "icd10": "I67.89",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Risk factor modification: blood pressure and lipid control."
                  }
                }
                """,
        ],
        ["PSYC_SESSION_V1"] =
        [
            """
                {
                  "chiefComplaint": "Persistent low mood and anhedonia for 3 months",
                  "mentalStateExam": {
                    "mood": "depressed",
                    "affect": "Constricted, tearful",
                    "orientation": "Oriented x4",
                    "thoughtProcess": "Linear, goal-directed",
                    "perception": "No hallucinations",
                    "cognition": "Intact",
                    "insight": "Good",
                    "sessionNotes": "Patient describes worsening depressive symptoms following job loss. Sleep disrupted. Appetite decreased. Denies suicidal ideation."
                  },
                  "anxietyLevel": 6,
                  "sleepPattern": "Initial insomnia — 2-3 hours to fall asleep",
                  "therapyType": "Cognitive behavioral therapy",
                  "riskAssessment": "Low risk — no suicidal or homicidal ideation",
                  "diagnosis": {
                    "description": "Major depressive disorder — moderate, single episode",
                    "icd10": "F32.1",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Weekly CBT sessions. SSRI initiated. Follow-up in 2 weeks.",
                    "medications": [
                      {
                        "name": "Sertraline",
                        "dose": "50mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ],
                    "nextAppointment": "2026-06-16"
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Follow-up — medication adjustment for generalized anxiety",
                  "mentalStateExam": {
                    "mood": "anxious",
                    "affect": "Tense but cooperative",
                    "orientation": "Oriented x4",
                    "thoughtProcess": "Ruminative but logical",
                    "perception": "No perceptual disturbances",
                    "cognition": "Intact",
                    "insight": "Fair",
                    "sessionNotes": "Patient reports partial improvement with current SSRI. Residual somatic anxiety (chest tightness, restlessness). Sleep improved slightly. Dose increase discussed and agreed upon."
                  },
                  "anxietyLevel": 5,
                  "sleepPattern": "Improved — occasional early awakening",
                  "currentMedications": [
                    "Escitalopram 10mg"
                  ],
                  "diagnosis": {
                    "description": "Generalized anxiety disorder",
                    "icd10": "F41.1",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Increase escitalopram to 15mg. Continue relaxation techniques. Follow-up in 4 weeks.",
                    "medications": [
                      {
                        "name": "Escitalopram",
                        "dose": "15mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
        ],
        ["PSYC_RISK_V1"] =
        [
            """
                {
                  "riskAssessment": {
                    "riskLevel": "moderate",
                    "suicidalIdeation": true,
                    "homicidalIdeation": false,
                    "triggers": [
                      "Social isolation",
                      "Financial stress",
                      "Recent breakup"
                    ],
                    "protectiveFactors": [
                      "Supportive family",
                      "Employment",
                      "Engaged in therapy"
                    ],
                    "safetyPlan": "Patient agreed to contact crisis line if urges escalate. Emergency contacts updated. Firearms removed from home."
                  },
                  "diagnosis": {
                    "description": "Adjustment disorder with depressed mood — suicidal ideation present",
                    "icd10": "F43.21",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Increase session frequency to twice weekly. Safety plan reviewed and signed. Consider psychiatric hospitalization if symptoms escalate."
                  }
                }
                """,
        ],
        ["URO_CONSULT_V1"] =
        [
            """
                {
                  "chiefComplaint": "Difficulty urinating and weak stream",
                  "biologicalSex": "male",
                  "urinarySymptoms": {
                    "description": "Progressive obstructive voiding symptoms over 6 months",
                    "urinaryFrequency": "Every 2 hours",
                    "urgency": true,
                    "dysuria": false,
                    "hematuria": false,
                    "incontinence": false,
                    "nocturia": true,
                    "weakStream": true,
                    "retention": false
                  },
                  "urologicalExam": {
                    "flankMasses": false,
                    "lumbosacralTenderness": false,
                    "maleExam": {
                      "prostateExam": "Smooth, mildly enlarged, non-tender",
                      "psa": 4.2,
                      "testicularExam": {
                        "left": "Normal",
                        "right": "Normal",
                        "tenderness": false,
                        "masses": false
                      }
                    }
                  },
                  "diagnosis": {
                    "description": "Benign prostatic hyperplasia",
                    "icd10": "N40.1",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Alpha-blocker initiated. Re-evaluate PSA in 3 months.",
                    "medications": [
                      {
                        "name": "Tamsulosin",
                        "dose": "0.4mg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Recurrent urinary tract infections",
                  "biologicalSex": "female",
                  "urinarySymptoms": {
                    "description": "Third UTI episode in 6 months",
                    "urinaryFrequency": "Every 1 hour during episodes",
                    "urgency": true,
                    "dysuria": true,
                    "hematuria": false,
                    "incontinence": false,
                    "nocturia": false,
                    "weakStream": false,
                    "retention": false
                  },
                  "urinalysis": {
                    "leukocytes": "Positive",
                    "nitrites": true,
                    "proteins": "Trace",
                    "blood": "Negative",
                    "bacteria": "Moderate",
                    "glucose": "Negative",
                    "ph": 6.5,
                    "appearance": "Turbid"
                  },
                  "diagnosis": {
                    "description": "Recurrent urinary tract infection",
                    "icd10": "N39.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Complete antibiotic course. Increase fluid intake. Prophylactic evaluation if recurrence persists.",
                    "medications": [
                      {
                        "name": "Nitrofurantoin",
                        "dose": "100mg",
                        "frequency": "Twice daily",
                        "duration": "7 days"
                      }
                    ]
                  }
                }
                """,
        ],
        ["ONCO_FOLLOWUP_V1"] =
        [
            """
                {
                  "chiefComplaint": "Scheduled follow-up — breast cancer, cycle 4 of chemotherapy",
                  "cancerStage": {
                    "tnmT": "T2",
                    "tnmN": "N1",
                    "tnmM": "M0",
                    "stage": "IIB",
                    "subtype": "Invasive ductal carcinoma"
                  },
                  "metastasis": false,
                  "performanceStatus": 1,
                  "weightChange": "Lost 2kg since last visit",
                  "oncologySummary": {
                    "treatmentType": "chemotherapy",
                    "treatmentIntent": "curative",
                    "treatmentResponse": "partial",
                    "currentCycle": 4,
                    "toxicityGrade": 2,
                    "sideEffects": [
                      "Nausea",
                      "Fatigue",
                      "Mild neuropathy"
                    ]
                  },
                  "tumorMarkers": [
                    {
                      "name": "CA 15-3",
                      "value": 28.5,
                      "unit": "U/mL",
                      "trend": "falling"
                    }
                  ],
                  "diagnosis": {
                    "description": "Invasive ductal carcinoma — partial response to chemotherapy",
                    "icd10": "C50.9",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Continue current regimen. Anti-emetic prophylaxis adjusted. Next cycle in 21 days.",
                    "medications": [
                      {
                        "name": "Ondansetron",
                        "dose": "8mg",
                        "frequency": "As needed",
                        "duration": "Per cycle"
                      }
                    ]
                  }
                }
                """,
        ],
        ["ENDO_HORMONAL_V1"] =
        [
            """
                {
                  "chiefComplaint": "Fatigue, weight gain, and cold intolerance",
                  "biologicalSex": "female",
                  "vitalSigns": {
                    "bloodPressure": "118/78",
                    "heartRate": 62,
                    "temperature": 36.2,
                    "weight": 78,
                    "height": 162
                  },
                  "bmi": 29.7,
                  "thyroidExam": "Diffusely enlarged, non-tender, no nodules",
                  "metabolicPanel": {
                    "glucose": {
                      "value": 95,
                      "unit": "mg/dL",
                      "referenceRange": "70-100"
                    },
                    "hba1c": {
                      "value": 5.4,
                      "unit": "%",
                      "referenceRange": "< 5.7"
                    }
                  },
                  "thyroidPanel": {
                    "tsh": {
                      "value": 12.8,
                      "unit": "mIU/L",
                      "referenceRange": "0.4-4.0"
                    },
                    "t3": {
                      "value": 0.8,
                      "unit": "ng/dL",
                      "referenceRange": "0.8-2.0"
                    },
                    "t4": {
                      "value": 0.5,
                      "unit": "ng/dL",
                      "referenceRange": "0.8-1.8"
                    }
                  },
                  "diagnosis": {
                    "description": "Primary hypothyroidism",
                    "icd10": "E03.9",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Levothyroxine initiated. Recheck TSH in 6 weeks.",
                    "medications": [
                      {
                        "name": "Levothyroxine",
                        "dose": "50mcg",
                        "frequency": "Once daily — fasting",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Polyuria and polydipsia — newly diagnosed diabetes",
                  "biologicalSex": "male",
                  "vitalSigns": {
                    "bloodPressure": "132/86",
                    "heartRate": 78,
                    "temperature": 36.6,
                    "weight": 92,
                    "height": 175
                  },
                  "bmi": 30,
                  "weightHistory": "Gained 8kg over 2 years",
                  "metabolicPanel": {
                    "glucose": {
                      "value": 210,
                      "unit": "mg/dL",
                      "referenceRange": "70-100"
                    },
                    "hba1c": {
                      "value": 8.9,
                      "unit": "%",
                      "referenceRange": "< 5.7"
                    },
                    "insulin": {
                      "value": 18,
                      "unit": "µIU/mL",
                      "referenceRange": "2.6-24.9"
                    }
                  },
                  "diagnosis": {
                    "description": "Type 2 diabetes mellitus — newly diagnosed, uncontrolled",
                    "icd10": "E11.65",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Dietary counseling, exercise program, Metformin initiated. Follow-up in 3 months.",
                    "medications": [
                      {
                        "name": "Metformin",
                        "dose": "500mg",
                        "frequency": "Twice daily with meals",
                        "duration": "Ongoing"
                      }
                    ]
                  }
                }
                """,
        ],
        ["GAST_DIGEST_V1"] =
        [
            """
                {
                  "chiefComplaint": "Epigastric burning and acid reflux",
                  "digestiveSymptoms": {
                    "description": "Postprandial epigastric burning radiating to chest",
                    "nausea": true,
                    "vomiting": false,
                    "diarrhea": false,
                    "constipation": false,
                    "rectalBleeding": false,
                    "bloating": true,
                    "reflux": true,
                    "weightLoss": false,
                    "appetiteChanges": "Decreased due to symptoms",
                    "foodRelation": "Worse after spicy or fatty foods"
                  },
                  "abdominalPain": {
                    "present": true,
                    "location": "epigastric",
                    "type": "Burning",
                    "severity": 6,
                    "timing": "Postprandial"
                  },
                  "abdomenExam": {
                    "tenderness": "Mild epigastric tenderness",
                    "organomegaly": "None",
                    "bowelSounds": "Normal",
                    "ascites": false,
                    "rigidity": false
                  },
                  "diagnosis": {
                    "description": "Gastroesophageal reflux disease",
                    "icd10": "K21.0",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Dietary modifications, elevate head of bed. PPI initiated.",
                    "medications": [
                      {
                        "name": "Omeprazole",
                        "dose": "20mg",
                        "frequency": "Once daily before breakfast",
                        "duration": "8 weeks"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Chronic intermittent diarrhea and abdominal cramping",
                  "digestiveSymptoms": {
                    "description": "Alternating diarrhea and constipation with bloating",
                    "nausea": false,
                    "vomiting": false,
                    "diarrhea": true,
                    "constipation": true,
                    "rectalBleeding": false,
                    "bloating": true,
                    "reflux": false,
                    "weightLoss": false,
                    "stoolCharacteristics": "Loose, sometimes mucoid"
                  },
                  "abdominalPain": {
                    "present": true,
                    "location": "periumbilical",
                    "type": "Cramping",
                    "severity": 4,
                    "timing": "Before bowel movements"
                  },
                  "abdomenExam": {
                    "tenderness": "Mild diffuse",
                    "organomegaly": "None",
                    "bowelSounds": "Hyperactive",
                    "ascites": false,
                    "rigidity": false
                  },
                  "diagnosis": {
                    "description": "Irritable bowel syndrome — mixed type",
                    "icd10": "K58.2",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Low-FODMAP diet trial, stress management. Antispasmodic as needed.",
                    "medications": [
                      {
                        "name": "Hyoscine butylbromide",
                        "dose": "10mg",
                        "frequency": "Three times daily before meals",
                        "duration": "4 weeks"
                      }
                    ]
                  }
                }
                """,
        ],
        ["PULM_RESPIRATORY_V1"] =
        [
            """
                {
                  "chiefComplaint": "Chronic cough with exertional dyspnea",
                  "smokingHistory": {
                    "status": "former",
                    "packYears": 15
                  },
                  "respiratoryAssessment": {
                    "oxygenSaturation": 93,
                    "respiratoryRate": 20,
                    "breathingSounds": "Bilateral expiratory wheezes",
                    "dyspnea": true,
                    "cough": true,
                    "hemoptysis": false,
                    "wheezing": true,
                    "sputumProduction": "Mucoid, small volume",
                    "exerciseTolerance": "Limited to one flight of stairs",
                    "oxygenTherapy": false
                  },
                  "spirometryResults": {
                    "fev1": {
                      "value": 1.8,
                      "unit": "L",
                      "percentPredicted": 62
                    },
                    "fvc": {
                      "value": 3.2,
                      "unit": "L",
                      "percentPredicted": 85
                    },
                    "fev1FvcRatio": 0.56,
                    "interpretation": "obstructive"
                  },
                  "diagnosis": {
                    "description": "Chronic obstructive pulmonary disease — moderate (GOLD II)",
                    "icd10": "J44.9",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Smoking cessation reinforcement. Inhaler technique review. Pulmonary rehabilitation referral.",
                    "medications": [
                      {
                        "name": "Tiotropium",
                        "dose": "18mcg",
                        "frequency": "Once daily",
                        "duration": "Ongoing"
                      },
                      {
                        "name": "Salbutamol MDI",
                        "dose": "200mcg",
                        "frequency": "As needed",
                        "duration": "As needed"
                      }
                    ]
                  }
                }
                """,
            """
                {
                  "chiefComplaint": "Nocturnal cough and wheezing since childhood",
                  "smokingHistory": {
                    "status": "never"
                  },
                  "respiratoryAssessment": {
                    "oxygenSaturation": 97,
                    "respiratoryRate": 16,
                    "breathingSounds": "Scattered wheezes on forced expiration",
                    "dyspnea": false,
                    "cough": true,
                    "hemoptysis": false,
                    "wheezing": true,
                    "sputumProduction": "None",
                    "exerciseTolerance": "Good — mMRC grade 0",
                    "oxygenTherapy": false
                  },
                  "spirometryResults": {
                    "fev1": {
                      "value": 2.9,
                      "unit": "L",
                      "percentPredicted": 78
                    },
                    "fvc": {
                      "value": 3.8,
                      "unit": "L",
                      "percentPredicted": 92
                    },
                    "fev1FvcRatio": 0.76,
                    "interpretation": "normal"
                  },
                  "diagnosis": {
                    "description": "Persistent asthma — moderate",
                    "icd10": "J45.4",
                    "type": "definitive"
                  },
                  "treatmentPlan": {
                    "instructions": "Step-up therapy: add ICS/LABA combination. Asthma action plan reviewed.",
                    "medications": [
                      {
                        "name": "Budesonide/Formoterol",
                        "dose": "160/4.5mcg",
                        "frequency": "Twice daily",
                        "duration": "Ongoing"
                      },
                      {
                        "name": "Salbutamol MDI",
                        "dose": "200mcg",
                        "frequency": "As needed for rescue",
                        "duration": "As needed"
                      }
                    ]
                  }
                }
                """,
        ],
    };

    public static string GetSamplePayload(string templateCode, int index)
    {
        if (!SamplesByTemplateCode.TryGetValue(templateCode, out var samples))
            return """{"additionalNotes":"Clinical details recorded during consultation."}""";

        return samples[index % samples.Length];
    }
}
