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
    required this.idioma,
    required this.mascota,
  });

  final Session session;
  final IdiomaController idioma;
  final Pet mascota;

  @override
  State<PetPage> createState() => _PetPageState();
}

class _PetPageState extends State<PetPage> {
  late Future<(VaccinationCard, List<Visit>)> _datos;

  @override
  void initState() {
    super.initState();
    _datos = _cargar();
  }

  Future<(VaccinationCard, List<Visit>)> _cargar() async {
    final cartilla = VaccinationApi(widget.session.api).cartillaDe(widget.mascota.id);
    final historial = MedicalRecordsApi(widget.session.api).historialDe(widget.mascota.id);
    return (await cartilla, await historial);
  }

  @override
  Widget build(BuildContext context) {
    final textos = widget.idioma.textos;
    final clinica = widget.session.usuario?.clinicName;

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(widget.mascota.name),
              Text(
                '${textos.especie(widget.mascota.species)} · ${textos.edad(widget.mascota.ageInWeeks)}',
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
            tabs: [Tab(text: textos.cartilla), Tab(text: textos.historial)],
          ),
        ),
        body: FutureBuilder<(VaccinationCard, List<Visit>)>(
          future: _datos,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(32),
                  child: Text(textos.sinConexion, textAlign: TextAlign.center),
                ),
              );
            }

            final (cartilla, historial) = snapshot.data!;
            return TabBarView(
              children: [
                VaccinationCardTab(cartilla: cartilla, textos: textos, clinica: clinica),
                RecordTab(
                  atenciones: historial,
                  textos: textos,
                  clinica: clinica,
                  nombreMascota: widget.mascota.name,
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
