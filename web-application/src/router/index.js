import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../iam/application/auth.store';

/**
 * Recorrido dominante de la sección 4.2.5: se parte de la búsqueda, se abre la
 * ficha del paciente y desde allí se accede a su cartilla o a su historial
 * mediante pestañas. La profundidad máxima es de tres niveles.
 */
const routes = [
  {
    path: '/acceso',
    name: 'acceso',
    component: () => import('../iam/presentation/SignInView.vue'),
    meta: { publica: true, titulo: 'Iniciar sesión — VetPass' }
  },
  {
    path: '/',
    component: () => import('../shared/presentation/AppLayout.vue'),
    children: [
      { path: '', redirect: { name: 'pacientes' } },
      {
        path: 'pacientes',
        name: 'pacientes',
        component: () => import('../patients/presentation/PatientsView.vue'),
        meta: { titulo: 'Pacientes — VetPass' }
      },
      {
        path: 'pacientes/nuevo',
        name: 'nuevo-paciente',
        component: () => import('../patients/presentation/NewPatientView.vue'),
        meta: { titulo: 'Registrar mascota — VetPass' }
      },
      {
        path: 'pacientes/:petId',
        name: 'paciente',
        component: () => import('../patients/presentation/PatientRecordView.vue'),
        props: true,
        meta: { titulo: 'Cartilla de vacunación — VetPass' }
      },
      {
        path: 'pacientes/:petId/atenciones/nueva',
        name: 'nueva-atencion',
        component: () => import('../medical-records/presentation/NewVisitView.vue'),
        props: true,
        meta: { titulo: 'Nueva atención — VetPass' }
      },
      {
        path: 'clientes',
        name: 'clientes',
        component: () => import('../patients/presentation/ClientsView.vue'),
        meta: { titulo: 'Clientes — VetPass' }
      }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: { name: 'pacientes' } }
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 })
});

router.beforeEach((destino) => {
  const sesion = useAuthStore();

  if (!destino.meta.publica && !sesion.autenticado) {
    return { name: 'acceso', query: { destino: destino.fullPath } };
  }

  if (destino.name === 'acceso' && sesion.autenticado) {
    return { name: 'pacientes' };
  }

  return true;
});

router.afterEach((destino) => {
  // Las páginas de la aplicación no se indexan: contienen información clínica
  // de pacientes (sección 4.2.3).
  if (destino.meta.titulo) document.title = destino.meta.titulo;
});
