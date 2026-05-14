import { EntityListPage } from '../../components/entity/CrudPages';
import { studentConfig } from '../../config/entities';

export function StudentsListPage() {
  return <EntityListPage config={studentConfig} />;
}