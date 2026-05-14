import { EntityFormPage } from '../../components/entity/CrudPages';
import { studentConfig } from '../../config/entities';

export function StudentEditPage() {
  return <EntityFormPage config={studentConfig} mode="edit" />;
}