import { EntityFormPage } from '../../components/entity/CrudPages';
import { enrollmentConfig } from '../../config/entities';

export function EnrollmentCreatePage() {
  return <EntityFormPage config={enrollmentConfig} mode="create" />;
}