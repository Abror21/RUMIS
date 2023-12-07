import SortableTable, { SortableTableItem } from "@/app/components/SortableTable"
import { TColumn, createApplicationListExcelFile } from "@/app/utils/utils"
import { Modal } from "antd"
import { Dispatch, SetStateAction } from "react"

type ExportModalProps = {
    setModalOpen: Dispatch<SetStateAction<boolean>>
    initialColumns: {
        key: string,
        name: string,
        isEnabled: boolean
    }[]
    data: any[]
    exportFunction: (data: any[], columns: TColumn[]) => void
}

const ExportModal = ({setModalOpen, initialColumns, data, exportFunction}: ExportModalProps) => {
    const handleOk = (columns: SortableTableItem[]) => {
        const parsedColumns = columns.filter(col => col.isEnabled).map(col => ({...col, title: col.name}))
        exportFunction(data, parsedColumns)

        setModalOpen(false)
    }
    return (
        <Modal
            open={true}
            footer={false}
            onCancel={() => setModalOpen(false)}
        >
            <SortableTable 
              initialData={initialColumns} 
              onCancel={() => setModalOpen(false)}
              onSubmit={(columns: SortableTableItem[]) => handleOk(columns)}  
            />
        </Modal>
    )
}

export default ExportModal