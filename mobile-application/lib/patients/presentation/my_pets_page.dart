import 'package:flutter/material.dart';
import '../../iam/application/session.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/status_chip.dart';
import '../../shared/presentation/theme.dart';
import '../domain/pet.dart';
import '../infrastructure/patients_api.dart';
import 'pet_page.dart';

/// Pantalla inicial de la aplicación (US12-E2). Cada mascota ocupa una tarjeta
/// que encabeza su propio estado de cartilla, que es la respuesta directa a la
/// confusión entre animales que recogieron las entrevistas.
///
/// No incorpora buscador: el dueño gestiona un número reducido de mascotas y
/// el listado cabe en una pantalla (sección 4.2.4).
class MyPetsPage extends StatefulWidget {
  const MyPetsPage({super.key, required this.session, required this.idioma});

  final Session session;
  final IdiomaController idioma;

  @override
  State<MyPetsPage> createState() => _MyPetsPageState();
}

class _MyPetsPageState extends State<MyPetsPage> {
  late Future<List<Pet>> _mascotas;

  @override
  void initState() {
    super.initState();
    _mascotas = PatientsApi(widget.session.api).misMascotas();
  }

  Future<void> _recargar() async {
    setState(() => _mascotas = PatientsApi(widget.session.api).misMascotas());
    await _mascotas;
  }

  @override
  Widget build(BuildContext context) {
    final textos = widget.idioma.textos;

    return Scaffold(
      appBar: AppBar(title: Text(textos.misMascotas)),
      body: RefreshIndicator(
        onRefresh: _recargar,
        child: FutureBuilder<List<Pet>>(
          future: _mascotas,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return _Mensaje(texto: textos.sinConexion, icono: Icons.cloud_off);
            }

            final mascotas = snapshot.data ?? const <Pet>[];
            if (mascotas.isEmpty) {
              return _Mensaje(texto: textos.sinMascotas, icono: Icons.pets_outlined);
            }

            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: mascotas.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, indice) => _TarjetaMascota(
                mascota: mascotas[indice],
                textos: textos,
                onTap: () => Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => PetPage(
                    session: widget.session,
                    idioma: widget.idioma,
                    mascota: mascotas[indice],
                  ),
                )),
              ),
            );
          },
        ),
      ),
    );
  }
}

class _TarjetaMascota extends StatelessWidget {
  const _TarjetaMascota({required this.mascota, required this.textos, required this.onTap});

  final Pet mascota;
  final AppStrings textos;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final descripcion = [
      textos.especie(mascota.species),
      if (mascota.breed != null) mascota.breed!,
      textos.edad(mascota.ageInWeeks),
    ].join(' · ');

    return Card(
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              Container(
                width: 48,
                height: 48,
                decoration: const BoxDecoration(
                  color: VetPassColors.primarySurface,
                  shape: BoxShape.circle,
                ),
                // Una huella sirve para ambas especies; la especie se nombra
                // en el texto, que es donde el usuario la lee.
                child: const Icon(Icons.pets, color: VetPassColors.primary),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(mascota.name, style: Theme.of(context).textTheme.titleLarge),
                    const SizedBox(height: 2),
                    Text(descripcion,
                        style: Theme.of(context)
                            .textTheme
                            .bodyMedium
                            ?.copyWith(color: VetPassColors.neutral600)),
                    if (mascota.cardStatus != null) ...[
                      const SizedBox(height: 10),
                      StatusChip(estado: mascota.cardStatus!, textos: textos),
                    ],
                  ],
                ),
              ),
              const Icon(Icons.chevron_right, color: VetPassColors.neutral600),
            ],
          ),
        ),
      ),
    );
  }
}

class _Mensaje extends StatelessWidget {
  const _Mensaje({required this.texto, required this.icono});

  final String texto;
  final IconData icono;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(32),
      children: [
        const SizedBox(height: 80),
        Icon(icono, size: 48, color: VetPassColors.neutral600),
        const SizedBox(height: 16),
        Text(texto,
            textAlign: TextAlign.center,
            style: Theme.of(context)
                .textTheme
                .bodyLarge
                ?.copyWith(color: VetPassColors.neutral600)),
      ],
    );
  }
}
