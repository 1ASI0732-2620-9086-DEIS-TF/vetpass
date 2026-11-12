import 'package:flutter/material.dart';
import '../../iam/application/session.dart';
import '../../medical_records/domain/visit.dart';
import '../../medical_records/infrastructure/medical_records_api.dart';
import '../../medical_records/presentation/record_tab.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/theme.dart';
import '../../vaccination/domain/vaccination_card.dart';
import '../../vaccination/infrastructure/vaccination_api.dart';
import '../../vaccination/presentation/vaccination_card_tab.dart';
import '../domain/pet.dart';

/// Ficha de una mascota. Cartilla e historial son pestañas de la misma
/// pantalla y no pantallas encadenadas, de modo que la profundidad de
/// navegación no pase de tres niveles (sección 4.2.5).
class PetPage extends StatefulWidget {
  const PetPage({
    super.key,
    required this.session,
    required this.language,
    required this.pet,
  });

  final Session session;
  final LanguageController language;
  final Pet pet;

  @override
  State<PetPage> createState() => _PetPageState();
}

class _PetPageState extends State<PetPage> {
  late Future<(VaccinationCard, List<Visit>)> _details;

  @override
  void initState() {
    super.initState();
    _details = _load();
  }

  Future<(VaccinationCard, List<Visit>)> _load() async {
    final card = VaccinationApi(widget.session.api).cardOf(widget.pet.id);
    final history = MedicalRecordsApi(widget.session.api).historyOf(widget.pet.id);
    return (await card, await history);
  }

  @override
  Widget build(BuildContext context) {
    final texts = widget.language.texts;
    final clinic = widget.session.user?.clinicName;

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(widget.pet.name),
              Text(
                '${texts.species(widget.pet.species)} · ${texts.age(widget.pet.ageInWeeks)}',
                style: Theme.of(context)
                    .textTheme
                    .labelSmall
                    ?.copyWith(color: VetPassColors.neutral600),
              ),
            ],
          ),
          bottom: TabBar(
            labelColor: VetPassColors.primary,
            unselectedLabelColor: VetPassColors.neutral600,
            indicatorColor: VetPassColors.primary,
            tabs: [Tab(text: texts.card), Tab(text: texts.history)],
          ),
        ),
        body: FutureBuilder<(VaccinationCard, List<Visit>)>(
          future: _details,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(32),
                  child: Text(texts.offline, textAlign: TextAlign.center),
                ),
              );
            }

            final (card, history) = snapshot.data!;
            return TabBarView(
              children: [
                VaccinationCardTab(card: card, texts: texts, clinic: clinic),
                RecordTab(
                  visits: history,
                  texts: texts,
                  clinic: clinic,
                  petName: widget.pet.name,
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
