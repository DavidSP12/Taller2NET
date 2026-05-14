import { EntityListPage } from '../../components/entity/CrudPages';
import { attendanceConfig } from '../../config/entities';

export function AttendanceListPage() {
  return <EntityListPage config={attendanceConfig} />;
}