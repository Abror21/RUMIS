import { useSession } from "next-auth/react"
import ApplicationDynamicDiagram from "./diagrams/ApplicationDynamicDiagram"
import OpenedApplicationsDiagram from "./diagrams/OpenedApplicationsDiagram"
import ResourceDiagram from "./diagrams/ResourceDiagram"
import UnprocessedApplicationList from "./lists/UnprocessedApplicationList"
import { getServerSession } from "next-auth"
import { authOptions } from "@/lib/auth"

const Dashboard = async () => {
    const session = await getServerSession(authOptions)

    return (
        <div className="grid grid-cols-2 gap-y-10">
            <OpenedApplicationsDiagram />
            {session && session?.user?.permissionType !== 'Supervisor' && <UnprocessedApplicationList />}
            {session && session?.user?.permissionType !== 'Supervisor' && <ApplicationDynamicDiagram />}
            <ResourceDiagram />
        </div>
    )
}

export default Dashboard