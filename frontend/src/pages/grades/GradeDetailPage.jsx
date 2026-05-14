import { EntityDetailPage } from '../../components/entity/CrudPages';
import { gradeConfig } from '../../config/entities';

export function GradeDetailPage() {
  return <EntityDetailPage config={gradeConfig} />;
}