Feature: US16 - History and prescriptions from the mobile application
  As a pet owner, I want to review the visits and prescriptions of my pets from my phone,
  so that I remember what was indicated and when.

  Scenario: E1 - History of a pet
    Given the clinic staff is signed in
    And today is "2026-10-07"
    And the staff registered a visit for "Kiara" on "2026-10-01" with reason "Control de peso" and diagnosis "Sano"
    And the owner "valeria.campos@correo.com" is signed in
    When the user requests the history of "Kiara"
    Then the visits are, from the most recent
      | reason                                        |
      | Control de peso                               |
      | Enrojecimiento y rascado en el pliegue facial |

  Scenario: E2 - Prescription of a visit
    Given the owner "valeria.campos@correo.com" is signed in
    When the user opens the visit of "Kiara" with reason "Enrojecimiento y rascado en el pliegue facial"
    Then the prescription contains
      | medication               | dosage                                  | duration |
      | Clorhexidina 2% solución | Aplicar en el pliegue dos veces al día. | 10 días  |
      | Cefalexina 250 mg        | Media tableta cada 12 horas.            | 7 días   |
