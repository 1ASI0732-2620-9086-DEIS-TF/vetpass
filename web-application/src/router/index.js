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
    name: 'access',
    component: () => import('../iam/presentation/SignInView.vue'),
    meta: { public: true, title: 'Iniciar sesión — VetPass' }
  },
  {
    path: '/',
    component: () => import('../shared/presentation/AppLayout.vue'),
    children: [
      { path: '', redirect: { name: 'patients' } },
      {
        path: 'pacientes',
        name: 'patients',
        component: () => import('../patients/presentation/PatientsView.vue'),
        meta: { title: 'Pacientes — VetPass' }
      },
      {
        path: 'pacientes/nuevo',
        name: 'new-patient',
        component: () => import('../patients/presentation/NewPatientView.vue'),
        meta: { title: 'Registrar mascota — VetPass' }
      },
      {
        path: 'pacientes/:petId',
        name: 'patient',
        component: () => import('../patients/presentation/PatientRecordView.vue'),
        props: true,
        meta: { title: 'Cartilla de vacunación — VetPass' }
      },
      {
        path: 'pacientes/:petId/atenciones/nueva',
        name: 'new-visit',
        component: () => import('../medical-records/presentation/NewVisitView.vue'),
        props: true,
        meta: { title: 'Nueva atención — VetPass' }
      },
      {
        path: 'clientes',
        name: 'clients',
        component: () => import('../patients/presentation/ClientsView.vue'),
        meta: { title: 'Clientes — VetPass' }
      }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: { name: 'patients' } }
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 })
});

router.beforeEach((destination) => {
  const session = useAuthStore();

  if (!destination.meta.public && !session.authenticated) {
    return { name: 'access', query: { destination: destination.fullPath } };
  }

  if (destination.name === 'access' && session.authenticated) {
    return { name: 'patients' };
  }

  return true;
});

router.afterEach((destination) => {
  // Las páginas de la aplicación no se indexan: contienen información clínica
  // de pacientes (sección 4.2.3).
  if (destination.meta.title) document.title = destination.meta.title;
});
