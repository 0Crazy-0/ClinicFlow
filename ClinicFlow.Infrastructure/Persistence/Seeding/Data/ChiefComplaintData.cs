namespace ClinicFlow.Infrastructure.Persistence.Seeding.Data;

/// <summary>
/// Provides clinically meaningful chief complaints organized by medical specialty for database seeding.
/// </summary>
public static class ChiefComplaintData
{
    public static IReadOnlyDictionary<string, string[]> GetBySpecialty() => _complaintsBySpecialty;

    private static readonly Dictionary<string, string[]> _complaintsBySpecialty = new()
    {
        ["General Medicine"] =
        [
            "Persistent headache and fatigue for the past two weeks",
            "Recurring fever and body aches",
            "Unexplained weight loss over the last month",
            "Chronic cough that has not resolved with over-the-counter medication",
            "General malaise and loss of appetite",
            "Dizziness and occasional fainting episodes",
            "Persistent sore throat and difficulty swallowing",
            "Joint pain and morning stiffness",
            "Routine annual health checkup",
            "Follow-up for hypertension and medication review",
        ],
        ["Pediatrics"] =
        [
            "Child has had a persistent cough and runny nose for five days",
            "Recurrent ear infections and hearing difficulties",
            "Routine well-child developmental checkup and vaccination review",
            "Failure to gain weight appropriately for age",
            "Skin rash and intermittent fever in a 3-year-old",
            "Behavioral changes and difficulty concentrating at school",
            "Chronic abdominal pain after meals",
            "Wheezing episodes triggered by physical activity",
        ],
        ["Cardiology"] =
        [
            "Chest pain radiating to the left arm during exertion",
            "Palpitations and irregular heartbeat at rest",
            "Shortness of breath when climbing stairs",
            "Swollen ankles and feet with progressive edema",
            "Dizziness and near-syncope episodes",
            "Follow-up after recent echocardiogram results",
            "Family history of heart disease: cardiovascular risk screening",
            "Persistent elevated blood pressure despite medication",
        ],
        ["Dermatology"] =
        [
            "New mole that has changed in color and size over two months",
            "Persistent itchy rash on forearms and neck",
            "Severe acne unresponsive to topical treatments",
            "Chronic eczema flare-up with cracking and bleeding",
            "Suspicious pigmented lesion on the upper back",
            "Hair loss in patches on the scalp",
            "Recurrent hives and urticaria of unknown cause",
            "Scaling and redness on elbows and knees consistent with psoriasis",
        ],
        ["Gynaecology"] =
        [
            "Irregular menstrual cycles with heavy bleeding",
            "Pelvic pain and abnormal vaginal discharge",
            "Routine prenatal care visit: second trimester follow-up",
            "Missed period with positive home pregnancy test",
            "Severe menstrual cramps interfering with daily activities",
            "Breast tenderness and lumps detected during self-exam",
            "Contraceptive counseling and method selection",
            "Post-menopausal bleeding and hot flashes",
        ],
        ["Ophthalmology"] =
        [
            "Progressive blurry vision in both eyes over six months",
            "Sudden floaters and flashes of light in the right eye",
            "Chronic dry eyes with burning and irritation",
            "Routine glaucoma follow-up and intraocular pressure check",
            "Red eye with discharge and sensitivity to light",
            "Difficulty reading small print despite current prescription",
            "Eye strain and headaches from prolonged screen use",
            "Follow-up after cataract surgery: post-operative evaluation",
        ],
        ["Orthopedics"] =
        [
            "Persistent lower back pain after lifting heavy objects",
            "Knee swelling and instability after a sports injury",
            "Shoulder pain with limited range of motion",
            "Wrist pain and numbness suggestive of carpal tunnel",
            "Post-fracture follow-up: assessing bone healing progress",
            "Hip pain that worsens with walking and stair climbing",
            "Chronic neck stiffness and radiating arm pain",
            "Ankle sprain that has not improved after three weeks",
        ],
        ["Otolaryngology"] =
        [
            "Persistent nasal congestion and facial pressure for three weeks",
            "Progressive hearing loss in the left ear",
            "Recurring sore throat and difficulty swallowing",
            "Tinnitus and ringing in both ears",
            "Chronic sinusitis unresponsive to antibiotics",
            "Hoarseness and voice changes lasting over a month",
            "Dizziness and vertigo episodes with nausea",
            "Enlarged lymph nodes in the neck with ear pain",
        ],
        ["Neurology"] =
        [
            "Severe migraine with aura occurring multiple times per week",
            "Numbness and tingling in hands and feet",
            "Memory lapses and difficulty finding words",
            "Seizure episode: first-time occurrence in an adult",
            "Tremor in the right hand at rest",
            "Chronic dizziness and balance problems",
            "Sudden onset weakness on one side of the body",
            "Sleep disturbances with involuntary leg movements",
        ],
        ["Psychiatry"] =
        [
            "Persistent low mood and loss of interest in daily activities for over a month",
            "Panic attacks with heart racing and shortness of breath",
            "Difficulty sleeping and intrusive thoughts",
            "Medication follow-up for generalized anxiety disorder",
            "Social withdrawal and inability to concentrate at work",
            "Mood swings and irritability affecting personal relationships",
            "Compulsive behaviors interfering with daily routine",
            "Follow-up after recent change in antidepressant dosage",
        ],
        ["Urology"] =
        [
            "Frequent urination and burning sensation during urination",
            "Blood in urine noticed over the past week",
            "Difficulty starting urination and weak stream",
            "Recurrent urinary tract infections",
            "Annual prostate screening and PSA review",
            "Flank pain radiating to the groin: suspected kidney stone",
            "Nocturia: waking up multiple times at night to urinate",
            "Erectile dysfunction and decreased libido",
        ],
        ["Oncology"] =
        [
            "Follow-up after second cycle of chemotherapy: assessing tolerance",
            "New lymph node enlargement discovered during self-examination",
            "Unexplained weight loss and night sweats over three months",
            "Post-surgical oncology review and treatment planning",
            "Tumor marker results review and staging update",
            "Persistent fatigue and bone pain during treatment",
            "Family history of cancer: genetic counseling and screening",
            "Monitoring treatment response after immunotherapy",
        ],
        ["Endocrinology"] =
        [
            "Uncontrolled blood sugar despite insulin adjustment",
            "Unexplained weight gain and chronic fatigue: thyroid evaluation",
            "Heat intolerance and rapid heartbeat: suspected hyperthyroidism",
            "Follow-up for HbA1c results and diabetes management plan",
            "Hair loss and irregular periods: hormonal panel requested",
            "Adrenal fatigue symptoms with low morning energy",
            "Newly diagnosed Type 2 diabetes: initial management consultation",
            "Bone density concerns and calcium metabolism evaluation",
        ],
        ["Gastroenterology"] =
        [
            "Chronic abdominal pain and bloating after meals",
            "Persistent acid reflux unresponsive to proton pump inhibitors",
            "Rectal bleeding and changes in bowel habits",
            "Chronic diarrhea alternating with constipation: suspected IBS",
            "Nausea and vomiting with upper abdominal pain",
            "Follow-up after upper endoscopy findings",
            "Unexplained elevated liver enzymes on routine bloodwork",
            "Difficulty swallowing solid foods: progressive dysphagia",
        ],
        ["Pulmonology"] =
        [
            "Chronic cough with sputum production for over three months",
            "Shortness of breath at rest and during minimal exertion",
            "Wheezing and chest tightness: asthma exacerbation",
            "Follow-up spirometry review for COPD management",
            "Persistent chest pain with deep breathing: pleuritic in nature",
            "Daytime sleepiness and loud snoring: suspected sleep apnea",
            "Hemoptysis: coughing up blood-streaked sputum",
            "Smoking cessation counseling and lung function baseline",
        ],
    };
}
