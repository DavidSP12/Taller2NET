import { EntityFormPage } from '../../components/entity/CrudPages';
import { studentConfig } from '../../config/entities';

export function StudentCreatePage() {
  return <EntityFormPage config={studentConfig} mode="create" />;
}