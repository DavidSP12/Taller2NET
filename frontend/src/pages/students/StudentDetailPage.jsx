import { EntityDetailPage } from '../../components/entity/CrudPages';
import { studentConfig } from '../../config/entities';

export function StudentDetailPage() {
  return <EntityDetailPage config={studentConfig} />;
}