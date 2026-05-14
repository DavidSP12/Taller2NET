import { EntityFormPage } from '../../components/entity/CrudPages';
import { enrollmentConfig } from '../../config/entities';

export function EnrollmentEditPage() {
  return <EntityFormPage config={enrollmentConfig} mode="edit" />;
}