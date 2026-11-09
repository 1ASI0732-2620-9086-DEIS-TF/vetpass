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
  const MyPetsPage({super.key, required this.session, required this.language});

  final Session session;
  final LanguageController language;

  @override
  State<MyPetsPage> createState() => _MyPetsPageState();
}

class _MyPetsPageState extends State<MyPetsPage> {
  late Future<List<Pet>> _pets;

  @override
  void initState() {
    super.initState();
    _pets = PatientsApi(widget.session.api).myPets();
  }

  Future<void> _reload() async {
    setState(() => _pets = PatientsApi(widget.session.api).myPets());
    await _pets;
  }

  @override
  Widget build(BuildContext context) {
    final texts = widget.language.texts;

    return Scaffold(
      appBar: AppBar(title: Text(texts.myPets)),
      body: RefreshIndicator(
        onRefresh: _reload,
        child: FutureBuilder<List<Pet>>(
          future: _pets,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return _Message(text: texts.offline, icon: Icons.cloud_off);
            }

            final pets = snapshot.data ?? const <Pet>[];
            if (pets.isEmpty) {
              return _Message(text: texts.noPets, icon: Icons.pets_outlined);
            }

            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: pets.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, index) => _PetCard(
                pet: pets[index],
                texts: texts,
                onTap: () => Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => PetPage(
                    session: widget.session,
                    language: widget.language,
                    pet: pets[index],
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

class _PetCard extends StatelessWidget {
  const _PetCard({required this.pet, required this.texts, required this.onTap});

  final Pet pet;
  final AppStrings texts;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final description = [
      texts.species(pet.species),
      if (pet.breed != null) pet.breed!,
      texts.age(pet.ageInWeeks),
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
                    Text(pet.name, style: Theme.of(context).textTheme.titleLarge),
                    const SizedBox(height: 2),
                    Text(description,
                        style: Theme.of(context)
                            .textTheme
                            .bodyMedium
                            ?.copyWith(color: VetPassColors.neutral600)),
                    if (pet.cardStatus != null) ...[
                      const SizedBox(height: 10),
                      StatusChip(status: pet.cardStatus!, texts: texts),
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

class _Message extends StatelessWidget {
  const _Message({required this.text, required this.icon});

  final String text;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(32),
      children: [
        const SizedBox(height: 80),
        Icon(icon, size: 48, color: VetPassColors.neutral600),
        const SizedBox(height: 16),
        Text(text,
            textAlign: TextAlign.center,
            style: Theme.of(context)
                .textTheme
                .bodyLarge
                ?.copyWith(color: VetPassColors.neutral600)),
      ],
    );
  }
}
