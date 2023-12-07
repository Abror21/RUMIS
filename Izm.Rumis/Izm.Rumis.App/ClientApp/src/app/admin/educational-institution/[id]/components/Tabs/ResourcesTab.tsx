'use client'

import SearchSelectInput from "@/app/components/searchSelectInput"
import { Form } from "antd"

const ResourcesTab = () => {
    return (
        <>
            <Form.Item>
                <SearchSelectInput placeholder="Please select" mode="multiple" style={{ minWidth: '250px' }} options={[{ label: 'label', value: 'value' }, { label: 'label2', value: 'value2' }]} />
            </Form.Item>
            <Form.Item>
                <SearchSelectInput placeholder="Please select" mode="multiple" style={{ minWidth: '250px' }} options={[{ label: 'label', value: 'value' }, { label: 'label2', value: 'value2' }]} />
            </Form.Item>
            <Form.Item>
                <SearchSelectInput placeholder="Please select" mode="multiple" style={{ minWidth: '250px' }} options={[{ label: 'label', value: 'value' }, { label: 'label2', value: 'value2' }]} />
            </Form.Item>
            <Form.Item>
                <SearchSelectInput placeholder="Please select" mode="multiple" style={{ minWidth: '250px' }} options={[{ label: 'label', value: 'value' }, { label: 'label2', value: 'value2' }]} />
            </Form.Item>
        </>
    )
}

export default ResourcesTab