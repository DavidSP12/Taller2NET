import { EntityFormPage } from '../../components/entity/CrudPages';
import { gradeConfig } from '../../config/entities';

export function GradeCreatePage() {
  return <EntityFormPage config={gradeConfig} mode="create" />;
}