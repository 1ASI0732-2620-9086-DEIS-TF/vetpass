/**
 * English labels. The terms come from the table in section 4.2.2 of the
 * report: Pacientes → Patients, Cartilla → Vaccination Card, Historial →
 * Record, Atención → Visit, Receta → Prescription.
 */
export default {
  brand: { name: 'VetPass', studio: 'by PawCode Studio' },

  nav: {
    patients: 'Patients',
    clients: 'Clients',
    cards: 'Vaccination Cards',
    searchPatient: 'Search patient',
    signOut: 'Sign out',
    language: 'Language'
  },

  access: {
    title: 'Your patient’s complete file, the moment they walk in',
    subtitle: 'Vaccination card and veterinary record of dogs and cats, in one place and free of paper.',
    header: 'Sign in to your clinic',
    note: 'Access restricted to clinic staff.',
    email: 'Email address',
    password: 'Password',
    signIn: 'Sign in',
    help: 'Forgot your password? Write to support.',
    invalidCredentials: 'The email or the password do not match.',
    offline: 'The server could not be reached. Check that the API is running.'
  },

  patients: {
    title: 'Patients',
    register: 'Add pet',
    species: 'Species',
    cardStatus: 'Card status',
    allSpecies: 'All',
    allStatuses: 'All',
    count: 'patient | patients',
    pet: 'Pet',
    owner: 'Owner',
    lastVisit: 'Last visit',
    noResults: 'No patients match that criterion.',
    noResultsAction: 'If this is a new patient, register them.',
    noVisits: 'No visits',
    age: 'Age'
  },

  registration: {
    title: 'Add pet',
    clientDetails: 'Client details',
    petDetails: 'Pet details',
    existingClient: 'Existing client',
    newClient: 'New client',
    selectClient: 'Select a client',
    fullName: 'Full name',
    phone: 'Phone number',
    email: 'Email address',
    name: 'Name',
    breed: 'Breed',
    birthDate: 'Date of birth',
    sex: 'Sex',
    noticeTitle: 'The vaccination card will be generated automatically',
    notice: 'On saving, VetPass will create the card for {name} with the {species} vaccination schedule and calculate the due date of every dose.',
    noticeWithoutName: 'On saving, VetPass will create the card with the schedule of its species and calculate the due date of every dose.',
    save: 'Save patient',
    saved: '{name} was registered together with their vaccination card.',
    phoneHelp: '9-digit mobile, or landline with its area code.',
    invalidPhone: 'Enter a Peruvian number: a 9-digit mobile beginning with 9, or a landline with its area code (01 for Lima).',
    futureBirth: 'The date of birth cannot be later than today.',
    implausibleBirth: 'That date would make the pet {age} years old, above the {max} admitted for the {species} species. Please check the year.',
    clientAlreadyCreated: 'The client was registered. Correct the pet details and save again: it will be linked to the same client.',
    documentType: 'ID document',
    documentNumber: 'Document number',
    documentHelp: { Dni: '8-digit DNI.', ForeignerCard: '9 to 12 letters or digits.' },
    invalidDocument: { Dni: 'The DNI must have exactly 8 digits.', ForeignerCard: 'The foreigner card must have 9 to 12 letters or digits.' },
    duplicateClient: 'This document already belongs to {name}, registered at the clinic.',
    useClient: 'Use this client'
  },

  profile: {
    card: 'Vaccination Card',
    history: 'Record',
    owner: 'Owner',
    weeks: '{n} weeks',
    months: '{n} months',
    years: '{n} years | {n} year | {n} years'
  },

  // Calendar age: "2 months, 9 days", "3 years, 1 month".
  age: {
    days: '{n} days | {n} day | {n} days',
    months: '{n} months | {n} month | {n} months',
    years: '{n} years | {n} year | {n} years',
    join: '{a}, {b}',
    newborn: 'Newborn'
  },

  document: {
    short: { Dni: 'DNI', ForeignerCard: 'CE' },
    long: { Dni: 'DNI (national ID)', ForeignerCard: 'CE (foreigner card)' }
  },

  card: {
    schedule: '{species} vaccination schedule',
    nextDose: 'Next dose due: {date}',
    today: 'today, {date}',
    nothingPending: 'No doses of the schedule are pending.',
    vaccine: 'Vaccine',
    date: 'Date',
    batch: 'Batch',
    veterinarian: 'Administered by',
    status: 'Status',
    expected: 'Due: {date}',
    registerDose: 'Record dose',
    note: 'Doses follow the sequence of the schedule. The highlighted row is the one due today. Each vaccine is recorded in order; different vaccines can be given on the same day.',
    beforeTheDose: 'First, the {dose}'
  },

  dose: {
    title: 'Record an applied dose',
    vaccine: 'Vaccine',
    applicationDate: 'Date of application',
    batch: 'Batch',
    veterinarian: 'Administered by',
    valid: 'Meets the minimum age and the interval since the previous dose.',
    notYet: 'It cannot be recorded before {date}: the minimum age or the interval since the previous dose is not met yet.',
    future: 'The date of application cannot be later than today, {date}.',
    register: 'Record dose',
    another: 'Record another dose of the schedule',
    registered: 'Dose recorded in the card of {name}.',
    outOfOrder: 'The {dose} must be recorded before this one: each vaccine is given in order.',
    historical: 'If the dose was given on another date, enter it: the following doses of the series will be scheduled from today.'
  },

  history: {
    title: 'Recorded visits',
    new: 'New visit',
    withPrescription: 'With prescription',
    empty: 'This pet has no previous visits on record.',
    note: 'The most recent visits are shown first.',
    reason: 'Reason for the visit',
    findings: 'Findings',
    diagnosis: 'Diagnosis',
    treatment: 'Treatment',
    weight: 'Weight',
    prescription: 'Prescription'
  },

  visit: {
    title: 'New visit',
    date: 'Date',
    veterinarian: 'Attending veterinarian',
    weight: 'Weight (kg)',
    reason: 'Reason for the visit',
    findings: 'Findings',
    diagnosis: 'Diagnosis',
    treatment: 'Treatment',
    prescription: 'Prescription',
    medication: 'Medication',
    dosage: 'Dosage',
    duration: 'Duration',
    add: 'Add medication',
    remove: 'Remove medication',
    prescriptionNotice: 'The prescription will be attached to this visit and visible to {owner} in their mobile application.',
    save: 'Save visit',
    saved: 'Visit recorded in the history of {name}.'
  },

  clients: {
    title: 'Clients',
    name: 'Name',
    phone: 'Phone number',
    email: 'Email address',
    pets: 'Pets',
    noEmail: 'No email',
    access: 'Application access',
    withAccess: 'Has access',
    grantAccess: 'Grant access',
    dialogTitle: 'Grant access to the mobile application',
    dialogNote: 'The account for {name} will be created and the system will generate a temporary password. Hand it over at the reception desk: it is shown only once.',
    emailRequired: 'This client has no email on record. Enter one to create their account.',
    create: 'Create account',
    created: 'Account created for {name}',
    temporaryPassword: 'Temporary password',
    handOver: 'Write it down or copy it now: it will not be shown again.',
    copy: 'Copy',
    copied: 'Copied',
    document: 'ID document',
    password: 'Password',
    noAccess: 'No access',
    reset: 'Reset',
    resetTitle: 'Reset password',
    resetNote: 'A new temporary password will be generated for {name} and the current one will stop working. On signing in, the app will ask them to choose their own.',
    resetConfirm: 'Reset password',
    resetDone: 'Password reset for {name}',
    privacy: 'VetPass neither stores nor shows client passwords: they can only be replaced with a temporary one.'
  },

  status: {
    UpToDate: 'Up to date',
    Pending: 'Pending',
    Overdue: 'Overdue',
    Applied: 'Applied',
    card: { UpToDate: 'Card up to date', Pending: 'Card pending', Overdue: 'Card overdue' }
  },

  species: { Canine: 'Canine', Feline: 'Feline', canineMasculine: 'canine', felineMasculine: 'feline', canineFeminine: 'canine', felineFeminine: 'feline' },

  vaccines: {
    'Quíntuple': 'DHPP',
    'Antirrábica': 'Rabies',
    'Triple felina': 'FVRCP',
    'Leucemia felina': 'FeLV'
  },
  doseOrdinal: '{n} dose',
  doseBooster: 'annual booster',
  sex: { Male: 'Male', Female: 'Female' },

  common: {
    cancel: 'Cancel',
    save: 'Save',
    close: 'Close',
    loading: 'Loading…',
    required: 'This field is required.',
    unexpectedError: 'Something went wrong while processing the request.',
    back: 'Back',
    terms: 'Terms and conditions'
  }
};
