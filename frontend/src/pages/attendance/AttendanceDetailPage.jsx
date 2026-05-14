import { EntityDetailPage } from '../../components/entity/CrudPages';
import { attendanceConfig } from '../../config/entities';

export function AttendanceDetailPage() {
  return <EntityDetailPage config={attendanceConfig} />;
}