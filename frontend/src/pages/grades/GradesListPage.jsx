import { EntityListPage } from '../../components/entity/CrudPages';
import { gradeConfig } from '../../config/entities';

export function GradesListPage() {
  return <EntityListPage config={gradeConfig} />;
}